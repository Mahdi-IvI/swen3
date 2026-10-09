import { Component } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

interface DocumentItem {
  id: number;
  name: string;
  description: string;
  fileName: string;
  containingFolder: number | null;
}

interface TokenResponse { token: string; }
interface UserProfile { firstName: string; lastName: string; }

type Screen = 'login' | 'register' | 'documents' | 'profile';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html'
})
export class AppComponent {
  private readonly apiUrl = '/api';
  private token = '';
  screen: Screen = 'login';
  username = '';
  password = '';
  firstName = '';
  lastName = '';
  email = '';
  error = '';
  documents: DocumentItem[] = [];
  profile: UserProfile | null = null;
  searchTerm = '';

  constructor(private readonly http: HttpClient) {}

  get filteredDocuments(): DocumentItem[] {
    const term = this.searchTerm.trim().toLocaleLowerCase('de');
    if (!term) return this.documents;
    return this.documents.filter((document) =>
      `${document.name} ${document.description} ${document.fileName}`.toLocaleLowerCase('de').includes(term)
    );
  }

  showLogin(): void { this.screen = 'login'; this.error = ''; }
  showRegister(): void { this.screen = 'register'; this.error = ''; }

  async login(): Promise<void> {
    this.error = '';
    try {
      const response = await firstValueFrom(this.http.post<TokenResponse>(`${this.apiUrl}/auth/login`, {
        username: this.username, password: this.password
      }));
      await this.openDocuments(response.token);
    } catch (error) {
      this.error = error instanceof HttpErrorResponse && error.status === 0
        ? 'Backend nicht erreichbar.'
        : 'Benutzername oder Passwort ist falsch.';
    }
  }

  async register(): Promise<void> {
    this.error = '';
    try {
      const response = await firstValueFrom(this.http.post<TokenResponse>(`${this.apiUrl}/auth/register`, {
        username: this.username,
        password: this.password,
        email: this.email,
        firstName: this.firstName,
        lastName: this.lastName
      }));
      await this.openDocuments(response.token);
    } catch (error) {
      this.error = error instanceof HttpErrorResponse && error.status === 0
        ? 'Backend nicht erreichbar.'
        : error instanceof HttpErrorResponse && error.status === 409
          ? 'Benutzername oder E-Mail-Adresse existiert bereits.'
          : 'Registrierung fehlgeschlagen.';
    }
  }

  logout(): void {
    this.token = '';
    this.documents = [];
    this.profile = null;
    this.username = '';
    this.password = '';
    this.showLogin();
  }

  async showProfile(): Promise<void> {
    if (!this.token) return;
    try {
      this.profile = await firstValueFrom(this.http.get<UserProfile>(`${this.apiUrl}/auth/me`, {
        headers: new HttpHeaders({ Authorization: `Bearer ${this.token}` })
      }));
      this.screen = 'profile';
    } catch {
      this.error = 'Profil konnte nicht geladen werden.';
    }
  }

  showDocuments(): void { this.screen = 'documents'; }

  fileType(fileName: string): string {
    const extension = fileName.split('.').pop()?.toLowerCase();
    if (extension === 'pdf') return 'PDF';
    if (extension === 'doc' || extension === 'docx') return 'DOCX';
    if (extension === 'xls' || extension === 'xlsx') return 'XLSX';
    if (extension === 'ppt' || extension === 'pptx') return 'PPTX';
    if (extension === 'txt') return 'TXT';
    return extension?.toUpperCase() || 'DATEI';
  }

  private async openDocuments(token: string): Promise<void> {
    this.token = token;
    try {
      this.documents = await firstValueFrom(this.http.get<DocumentItem[]>(`${this.apiUrl}/documents`, {
        headers: new HttpHeaders({ Authorization: `Bearer ${token}` })
      }));
      this.error = '';
      this.screen = 'documents';
    } catch {
      this.token = '';
      this.error = 'Dateien konnten nicht geladen werden.';
    }
  }
}
