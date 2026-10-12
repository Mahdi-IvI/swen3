import {Component, Input} from '@angular/core';
import {CommonModule} from '@angular/common';

@Component({
    selector: 'app-user-profile',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './user-profile.component.html'
})
export class UserProfileComponent {
    @Input() firstName = '';
    @Input() lastName = '';
    @Input() documentCount = 0;
}
