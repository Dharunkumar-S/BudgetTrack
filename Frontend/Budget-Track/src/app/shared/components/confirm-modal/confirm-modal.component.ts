import { Component, Output, EventEmitter, Input } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-confirm-modal',
  standalone: true,
  imports: [NgClass],
  templateUrl: './confirm-modal.component.html'
})
export class ConfirmModalComponent {
  @Input() modalId = 'confirmModal';
  @Input() title = 'Confirm Action';
  @Input() message = 'Are you sure you want to proceed?';
  @Input() confirmLabel = 'Confirm';
  @Input() confirmClass = 'btn-danger';
  @Output() confirmed = new EventEmitter<void>();

  onConfirm(): void {
    this.confirmed.emit();
    // Close via Bootstrap JS (bootstrap.bundle already included)
    const el = document.getElementById(this.modalId);
    if (el) {
      const modal = (window as any).bootstrap?.Modal.getInstance(el);
      modal?.hide();
    }
  }
}
