import {Component, Input} from '@angular/core';
import {CommonModule} from '@angular/common';

export interface DocumentPropertiesData {
    name: string;
    description: string;
    fileName: string;
    sizeBytes?: number | null;
}

@Component({
    selector: 'app-document-properties',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './document-properties.component.html'
})
export class DocumentPropertiesComponent {
    @Input() document: DocumentPropertiesData | null = null;
    @Input() folderName = '';
    @Input() busy = false;
    @Input() error = '';

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
        if (sizeBytes == null || !Number.isFinite(sizeBytes) || sizeBytes < 0) return '';
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
}
