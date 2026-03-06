import { Component, input, computed } from '@angular/core';

@Component({
  selector: 'app-user-avatar',
  standalone: true,
  imports: [],
  templateUrl: './user-avatar.component.html',
  styleUrl: './user-avatar.component.css'
})
export class UserAvatarComponent {
  firstName = input.required<string>();
  lastName = input.required<string>();
  role = input<string>('');
  size = input<number>(32);

  initials = computed(() => {
    const fn = (this.firstName() || '').trim().charAt(0);
    const ln = (this.lastName() || '').trim().charAt(0);
    return (fn + ln).toUpperCase() || '?';
  });

  fontSize = computed(() => {
    const s = this.size();
    if (s <= 24) return 10;
    if (s <= 32) return 12;
    if (s <= 48) return 16;
    return Math.floor(s * 0.4);
  });

  avatarClass = computed(() => {
    switch ((this.role() || '').toLowerCase()) {
      case 'admin': return 'bg-admin';
      case 'manager': return 'bg-manager';
      case 'employee': return 'bg-employee';
      default: return 'bg-primary';
    }
  });
}
