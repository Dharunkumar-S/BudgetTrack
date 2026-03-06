import { Component, Input } from '@angular/core';
import { NgClass } from '@angular/common';

type StatusType = 'Active' | 'Closed' | 'Pending' | 'Approved' | 'Rejected' | 'Cancelled' | string;

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [NgClass],
  templateUrl: './status-badge.component.html'
})
export class StatusBadgeComponent {
  @Input() status: StatusType = '';
  @Input() label: string = '';

  get badgeClass(): string {
    const s = (this.status || '').toLowerCase();
    if (s === 'active' || s === 'approved') return 'badge-active';
    if (s === 'closed' || s === 'rejected') return 'badge-closed';
    if (s === 'pending') return 'badge-pending';
    if (s === 'cancelled') return 'badge-cancelled';
    if (s === 'expired') return 'badge-expired';
    if (s === 'over budget') return 'badge-overbudget';
    if (s === 'deleted') return 'badge-deleted';
    return 'badge bg-secondary';
  }

  get iconClass(): string {
    const s = (this.status || '').toLowerCase();
    if (s === 'active' || s === 'approved') return 'fas fa-check-circle';
    if (s === 'closed' || s === 'rejected') return 'fas fa-times-circle';
    if (s === 'pending') return 'fas fa-clock';
    if (s === 'cancelled') return 'fas fa-ban';
    if (s === 'expired') return 'fas fa-hourglass-end';
    if (s === 'over budget') return 'fas fa-exclamation-triangle';
    if (s === 'deleted') return 'fas fa-trash-alt';
    return 'fas fa-circle';
  }
}
