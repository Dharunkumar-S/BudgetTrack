import { Component, inject } from '@angular/core';
import { NgClass } from '@angular/common';
import { ToastService, Toast } from '@core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [NgClass],
  templateUrl: './toast-container.component.html',
  styleUrl: './toast-container.component.css'
})
export class ToastContainerComponent {
    toastService = inject(ToastService);

    getClass(toast: Toast): Record<string, boolean> {
        return {
            'toast-success': toast.type === 'success',
            'toast-error': toast.type === 'error',
            'toast-warning': toast.type === 'warning',
            'toast-info': toast.type === 'info',
        };
    }

    getIcon(toast: Toast): string {
        const icons: Record<string, string> = {
            success: 'fas fa-check-circle',
            error: 'fas fa-exclamation-circle',
            warning: 'fas fa-exclamation-triangle',
            info: 'fas fa-info-circle',
        };
        return icons[toast.type] ?? 'fas fa-info-circle';
    }
}
