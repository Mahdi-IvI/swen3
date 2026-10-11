import {Component, ElementRef, ViewChild} from '@angular/core';
import {HttpClient, HttpErrorResponse, HttpHeaders} from '@angular/common/http';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {firstValueFrom} from 'rxjs';

interface DocumentItem {
    id: number;
    name: string;
    description: string;
    fileName: string;
    sizeBytes?: number | null;
    containingFolder: number | null;
}

interface FolderItem {
    id: number;
    name: string;
    description: string;
    containingFolder: number | null;
}

interface TokenResponse {
    token: string;
}

interface UserProfile {
    firstName: string;
    lastName: string;
}

type Screen = 'login' | 'register' | 'documents' | 'profile';

@Component({
    selector: 'app-root',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './app.component.html'
})
export class AppComponent {
    @ViewChild('folderDialog', {static: true}) private folderDialog!: ElementRef<HTMLDialogElement>;
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
    folders: FolderItem[] = [];
    currentFolderId: number | null = null;
    editingFolderId: number | null = null;
    folderName = '';
    folderDescription = '';
    folderBusy = false;
    profile: UserProfile | null = null;
    searchTerm = '';

    constructor(private readonly http: HttpClient) {
    }

    get filteredDocuments(): DocumentItem[] {
        const term = this.searchTerm.trim().toLocaleLowerCase('de');
        const documents = this.documents.filter((document) => document.containingFolder === this.currentFolderId);
        if (!term) return documents;
        return documents.filter((document) =>
            `${document.name} ${document.description} ${document.fileName}`.toLocaleLowerCase('de').includes(term)
        );
    }

    get visibleFolders(): FolderItem[] {
        return this.folders.filter((folder) => folder.containingFolder === this.currentFolderId);
    }

    get currentFolder(): FolderItem | undefined {
        return this.folders.find((folder) => folder.id === this.currentFolderId);
    }

    openFolder(id: number | null): void {
        this.currentFolderId = id;
        this.searchTerm = '';
        this.error = '';
        this.cancelFolderForm();
    }

    showFolderForm(folder?: FolderItem): void {
        this.editingFolderId = folder?.id ?? null;
        this.folderName = folder?.name ?? '';
        this.folderDescription = folder?.description ?? '';
        this.error = '';
        this.folderDialog.nativeElement.showModal();
    }

    cancelFolderForm(): void {
        this.folderDialog.nativeElement.close();
        this.editingFolderId = null;
        this.folderName = '';
        this.folderDescription = '';
    }

    async saveFolder(): Promise<void> {
        if (this.folderBusy || !this.folderName.trim() || !this.folderDescription.trim()) return;
        this.folderBusy = true;
        this.error = '';
        const id = this.editingFolderId;
        const folder = {
            name: this.folderName.trim(),
            description: this.folderDescription.trim(),
            containingFolder: this.currentFolderId
        };
        try {
            const options = {headers: new HttpHeaders({Authorization: `Bearer ${this.token}`})};
            const saved = await firstValueFrom(id === null
                ? this.http.post<FolderItem>(`${this.apiUrl}/folders`, folder, options)
                : this.http.put<FolderItem>(`${this.apiUrl}/folders/${id}`, folder, options));
            this.folders = id === null
                ? [...this.folders, saved]
                : this.folders.map((item) => item.id === id ? saved : item);
            this.cancelFolderForm();
        } catch {
            this.error = 'Ordner konnte nicht gespeichert werden.';
        } finally {
            this.folderBusy = false;
        }
    }

    async deleteFolder(folder: FolderItem): Promise<void> {
        if (this.folderBusy || !window.confirm(`Ordner "${folder.name}" mit allen Unterordnern und Dokumenten löschen?`)) return;
        this.folderBusy = true;
        this.error = '';
        try {
            await firstValueFrom(this.http.delete(`${this.apiUrl}/folders/${folder.id}`, {
                headers: new HttpHeaders({Authorization: `Bearer ${this.token}`})
            }));
            this.cancelFolderForm();
            await this.loadContent();
        } catch {
            this.error = 'Ordner konnten nicht gelöscht oder neu geladen werden.';
        } finally {
            this.folderBusy = false;
        }
    }

    showLogin(): void {
        this.screen = 'login';
        this.error = '';
    }

    showRegister(): void {
        this.screen = 'register';
        this.error = '';
    }

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
        this.folders = [];
        this.currentFolderId = null;
        this.searchTerm = '';
        this.cancelFolderForm();
        this.profile = null;
        this.username = '';
        this.password = '';
        this.showLogin();
    }

    async showProfile(): Promise<void> {
        if (!this.token) return;
        try {
            this.profile = await firstValueFrom(this.http.get<UserProfile>(`${this.apiUrl}/auth/me`, {
                headers: new HttpHeaders({Authorization: `Bearer ${this.token}`})
            }));
            this.screen = 'profile';
        } catch {
            this.error = 'Profil konnte nicht geladen werden.';
        }
    }

    showDocuments(): void {
        this.screen = 'documents';
    }

    fileType(fileName: string): string {
        const extension = fileName.split('.').pop()?.toLowerCase();
        if (extension === 'pdf') return 'PDF';
        if (extension === 'doc' || extension === 'docx') return 'DOCX';
        if (extension === 'xls' || extension === 'xlsx') return 'XLSX';
        if (extension === 'ppt' || extension === 'pptx') return 'PPTX';
        if (extension === 'txt') return 'TXT';
        return extension?.toUpperCase() || 'DATEI';
    }

    formatFileSize(sizeBytes: number | null | undefined): string {
        if (sizeBytes == null || !Number.isFinite(sizeBytes) || sizeBytes < 0) return 'Größe nicht verfügbar';
        if (sizeBytes < 1024) return `${sizeBytes} Bytes`;
        const units = ['KB', 'MB', 'GB', 'TB'];
        let size = sizeBytes / 1024;
        let unit = 0;
        while (size >= 1024 && unit < units.length - 1) {
            size /= 1024;
            unit++;
        }
        return `${new Intl.NumberFormat('de-DE', {maximumFractionDigits: 1}).format(size)} ${units[unit]}`;
    }

    private async openDocuments(token: string): Promise<void> {
        this.token = token;
        try {
            await this.loadContent();
            this.error = '';
            this.screen = 'documents';
        } catch {
            this.token = '';
            this.error = 'Dateien und Ordner konnten nicht geladen werden.';
        }
    }

    private async loadContent(): Promise<void> {
        const options = {headers: new HttpHeaders({Authorization: `Bearer ${this.token}`})};
        const [documents, folders] = await Promise.all([
            firstValueFrom(this.http.get<DocumentItem[]>(`${this.apiUrl}/documents`, options)),
            firstValueFrom(this.http.get<FolderItem[]>(`${this.apiUrl}/folders`, options))
        ]);
        this.documents = documents;
        this.folders = folders;
    }
}
