import { Component, ViewEncapsulation } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-sms',
  templateUrl: './sms.component.html',
  styleUrls: ['./sms.component.scss'],
  // encapsulation: ViewEncapsulation.None,
})
export class SmsComponent {
  file: File | null = null;
  fileName: string | null = null;
  errorMessage: string | null = null;
  isUploading: boolean = false; // Track upload state
  isDownloading: boolean = false; // Track download state
  isFileUploaded: boolean = false; // Flag to track file upload completion
  isSending: boolean = false; // Track if messages are being sent
  successMessage: string | null = null; // Store success message
  isSendingMessages: boolean = false; // Track send message request status
  
  constructor(private http: HttpClient, private authService: AuthService) {}

  // Called when the user selects a file
  onFileChange(event: any): void {
    const file = event.target.files[0];
    if (file) {
      if (this.isValidFile(file)) {
        this.file = file;
        this.fileName = file.name;
        this.errorMessage = null;
      } else {
        this.file = null;
        this.fileName = null;
        this.errorMessage = 'Invalid file type. Please select an Excel file.';
      }
    }
  }

  // Check if the file is a valid Excel file
  private isValidFile(file: File): boolean {
    const allowedTypes = ['application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'application/vnd.ms-excel'];
    return allowedTypes.includes(file.type);
  }

  // Called when the user clicks the "Upload" button
  uploadFile(): void {
    if (!this.file) {
      return;
    }

    this.isUploading = true; // Set uploading flag
    this.errorMessage = null; // Reset any previous error message
    this.isFileUploaded = false; // Reset file uploaded flag if uploading again
    const formData = new FormData();
    formData.append('file', this.file, this.file.name);

    const token = this.authService.getToken();
    const headers = { Authorization: `Bearer ${token}` };

    this.http
      .post(`${environment.apiUrl}/Excel/OneNineTwo/upload`, formData, { headers })
      .subscribe(
        (response) => {
          console.log('File uploaded successfully', response);
          this.isFileUploaded = true; // Mark the file as uploaded
          this.isUploading = false; // Reset uploading flag after success
        },
        (error) => {
          console.error('Error uploading file', error);
          this.errorMessage = 'Error uploading file. Please try again.';
          this.isUploading = false; // Reset upload flag on error
        }
      );
  }

  downloadFile(): void {
    this.isDownloading = true; // Set downloading flag
    this.errorMessage = null; // Reset any error message

    const token = this.authService.getToken();
    const headers = { Authorization: `Bearer ${token}` };

    this.http
      .get(`${environment.apiUrl}/template/download?templateName=SMS`, {
        responseType: 'blob',
        headers,
      })
      .subscribe(
        (response) => {
          const url = window.URL.createObjectURL(response);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'SMS_API.xlsx'; // Set the default name for the downloaded file
          a.click();
          window.URL.revokeObjectURL(url);
          this.isDownloading = false; // Reset downloading flag
        },
        (error) => {
          console.error('Error downloading file', error);
          this.errorMessage = 'Error downloading file. Please try again.';
          this.isDownloading = false; // Reset downloading flag
        }
      );
  }

  sendMessages(): void {
    if (!this.isFileUploaded) {
      this.errorMessage = 'Please upload a file before sending messages.';
      return;
    }
    
    this.errorMessage = null;

    this.isSendingMessages = true; // Set sending flag
    this.isUploading = true; // Show uploading status
    const token = this.authService.getToken();
    const headers = { Authorization: `Bearer ${token}` };

    const url = `${environment.apiUrl}/Excel/OneNineTwo/insertdata?fileName=${this.fileName}&tableName=MessageOut`;
    console.log(`Sending request to: ${url}`);

    this.http
      .post(url, {}, { headers })
      .subscribe(
        (response: any) => {
          console.log('Messages sent successfully', response);
          this.isUploading = false;
          this.isSendingMessages = false; // Reset sending flag
          this.isFileUploaded = false; // Optionally reset after sending
          if (response.errorMessages && response.errorMessages.length > 0) {
            this.errorMessage = `Errors occurred: ${response.errorMessages.join(', ')}`;
          }
          else
          {
            this.successMessage = `Data added to database successfully. ${response.linesAdded} lines were added. Please verify with the administrator whether all the messages were sent successfully.`;
          }
        },
        (error) => {
          console.error('Error sending messages', error);
          this.errorMessage = 'Error sending messages. Please try again.';
          this.isUploading = false;
          this.isSendingMessages = false; // Reset sending flag on error
        }
      );
  }
  navigateToHome(): void {
    window.location.href = '/home';  // Adjust the URL to your home page
  }
}
