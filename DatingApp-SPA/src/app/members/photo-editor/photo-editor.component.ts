import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { FileUploader } from 'ng2-file-upload';
import { environment } from 'src/environments/environment';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() photos: Photo[];
  @Output() getMemberPhotoChange = new EventEmitter<string>(); // used to emit our photo url
  uploader: FileUploader;
  hasBaseDropZoneOver  = false;
  baseUrl = environment.apiUrl;
  currentMain: Photo;

  constructor(private authService: AuthService,
              private userService: UserService,
              private alertify: AlertifyService) { }

  ngOnInit() {
    this.initializeUploader();
  }

  fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  initializeUploader() {
    this.uploader = new FileUploader({
        url: this.baseUrl + 'users/' + this.authService.decodedToken.nameid + '/photos',
        authToken: 'Bearer ' + localStorage.getItem('token'),
        isHTML5: true,
        allowedFileType: ['image'],
        removeAfterUpload: true,
        autoUpload: false,
        maxFileSize: 10 * 1024 * 1024 // 10MB
    });

    this.uploader.onAfterAddingFile = (file) => { file.withCredentials = false; }; // to deal with an error if we did not have this
  
    this.uploader.onSuccessItem =(item, response, status, headers) => {
      if(response) {
        const res: Photo = JSON.parse(response); // parse text into object
        const photo = {
          id : res.id,
          url: res.url,
          dateAdded: res.dateAdded,
          description: res.description,
          isMain: res.isMain
        };
        this.photos.push(photo);

        // L.133 to update the photo for newly rgistered users
        if (photo.isMain) {
          this.authService.changeMemberPhoto(photo.url);
          this.authService.currentUser.photoUrl = photo.url;
          localStorage.setItem('user', JSON.stringify(this.authService.currentUser));
        }
      }
    };
  }

  setMainPhoto(photo: Photo) {
    this.userService.setMainPhoto(this.authService.decodedToken.nameid, photo.id)
      .subscribe(() => {
        // console.log('Successfully set to main');
        this.currentMain = this.photos.filter(p => p.isMain === true)[0];
        this.currentMain.isMain = false;
        photo.isMain = true;
        // this.getMemberPhotoChange.emit(photo.url); // works with the @Output property
        this.authService.changeMemberPhoto(photo.url); // L.119 instead of emiting photoUrl. to update the photo in card and navbar
        this.authService.currentUser.photoUrl = photo.url; // L.119 to add chanegd photo to localstorage
        localStorage.setItem('user', JSON.stringify(this.authService.currentUser));  // L.119 to add chanegd photo to localstorage
      }, error => {
          this.alertify.error(error);
      });
  }

  deletePhoto(id: number) { // L.121
    this.alertify.confirm('are you sure you want to delete this photo?', () => {  // on the callback
        this.userService.deletePhoto(this.authService.decodedToken.nameid, id).subscribe(() => {
          this.photos.splice(this.photos.findIndex(p => p.id === id), 1); // to remove photo from array
          this.alertify.success('Photo has been deleted');
        }, error => {
          this.alertify.error('Failed to delete teh photo');
        });
    });
  }
}
