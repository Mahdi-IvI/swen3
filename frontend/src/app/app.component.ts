import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface DocumentItem {
  id: number;
  name: string;
  description: string;
  fileName: string;
  containingFolder: number | null;
}

type Screen = 'login' | 'register' | 'documents';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html'
})
export class AppComponent {
  screen: Screen = 'login';
  username = 'admin';
  password = '';
  firstName = '';
  lastName = '';
  email = '';
  error = '';
  documents: DocumentItem[] = [];
  searchTerm = '';

  get filteredDocuments(): DocumentItem[] {
    const term = this.searchTerm.trim().toLocaleLowerCase('de');
    if (!term) return this.documents;
    return this.documents.filter((document) =>
      `${document.name} ${document.description} ${document.fileName}`.toLocaleLowerCase('de').includes(term)
    );
  }

  showLogin(): void { this.screen = 'login'; this.error = ''; }
  showRegister(): void { this.screen = 'register'; this.error = ''; }

  login(): void {
    if (this.username === 'admin' && this.password === 'admin123') {
      this.error = '';
      this.screen = 'documents';
      return;
    }

    this.error = 'Benutzername oder Passwort ist falsch.';
  }

  register(): void {
    this.error = 'Registrierung ist derzeit nicht verfügbar.';
  }

  logout(): void {
    this.documents = [];
    this.username = '';
    this.password = '';
    this.showLogin();
  }

  fileType(fileName: string): string {
    const extension = fileName.split('.').pop()?.toLowerCase();
    if (extension === 'pdf') return 'PDF';
    if (extension === 'doc' || extension === 'docx') return 'DOCX';
    if (extension === 'xls' || extension === 'xlsx') return 'XLSX';
    if (extension === 'ppt' || extension === 'pptx') return 'PPTX';
    return extension?.toUpperCase() || 'DATEI';
  }
}
