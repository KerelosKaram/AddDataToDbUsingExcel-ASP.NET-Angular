import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { AuthService } from 'src/app/auth/auth.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-elamir-customer-classification2',
  templateUrl: './elamir-customer-classification2.component.html',
  styleUrls: ['./elamir-customer-classification2.component.scss']
})
export class ElamirCustomerClassification2Component {

  file: File | null = null;
  fileName: string | null = null;
  errorMessage: string | null = null;
  isUploading: boolean = false; // Track upload state
  isDownloading: boolean = false; // Track download state
  isFileUploaded: boolean = false; // Flag to track file upload completion
  successMessage: string | null = null; // Store success message
  isSendingData: boolean = false; // Track send message request status
  isDeletingData: boolean = false; // Track send message request status
  
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
      .post(`${environment.apiUrl}/Excel/DbAx/upload`, formData, { headers })
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
      .get(`${environment.apiUrl}/template/download?templateName=ElamirCustomerClassification2`, {
        responseType: 'blob',
        headers,
      })
      .subscribe(
        (response) => {
          const url = window.URL.createObjectURL(response);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'IGPItemElamir_API.xlsx'; // Set the default name for the downloaded file
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

  addDataToTable(): void {
    if (!this.isFileUploaded) {
      this.errorMessage = 'Please upload a file before adding data to database.';
      return;
    }
    
    this.errorMessage = null;

    this.isSendingData = true; // Set sending flag
    this.isUploading = true; // Show uploading status
    const token = this.authService.getToken();
    const headers = { Authorization: `Bearer ${token}` };

    const url = `${environment.apiUrl}/Excel/DbAx/insertdata?fileName=${this.fileName}&tableName=ElamirCustomerClassification2`;
    console.log(`Sending request to: ${url}`);

    this.http
      .post(url, {}, { headers })
      .subscribe(
        (response: any) => {
          console.log('Data added to database successfully', response);
          this.isUploading = false;
          this.isSendingData = false; // Reset sending flag
          this.isFileUploaded = false; // Optionally reset after sending
          if (response.errorMessages && response.errorMessages.length > 0) {
            this.errorMessage = `Errors occurred: ${response.errorMessages.join(', ')}`;
          }
          else
          {
            this.successMessage = `Data added to database successfully. ${response.linesAdded} lines were added.`;
          }
        },
        (error) => {
          console.error('Error adding data to database', error);
          this.errorMessage = 'Error adding data to database. Please try again.';
          this.isUploading = false;
          this.isSendingData = false; // Reset sending flag on error
        }
      );
  }

  deleteTableFromDatabase(): void {
    this.errorMessage = null; // Reset error message
    this.successMessage = null; // Reset success message
  
    const token = this.authService.getToken();
    const headers = { Authorization: `Bearer ${token}` };
  
    const url = `${environment.apiUrl}/Excel/DbAx/deletedata?tableName=ElamirCustomerClassification2`;
    console.log(`Sending request to: ${url}`);
  
    // Optional: Add a loading state for the delete operation
    this.isDeletingData = true; // Reusing the uploading flag for status tracking
  
    this.http.get(url, { headers }).subscribe({
      next: (response) => {
        console.log('Table data deleted successfully', response);
        this.successMessage = 'Table data deleted successfully!';
        this.isDeletingData = false; // Reset loading flag
      },
      error: (error) => {
        console.error('Error deleting table data', error);
        this.errorMessage = 'Error deleting table data. Please try again.';
        this.isDeletingData = false; // Reset loading flag
      },
      complete: () => {
        console.log('Delete operation completed.');
      },
    });
  }

  navigateToHome(): void {
    window.location.href = '/home';  // Adjust the URL to your home page
  }
}
