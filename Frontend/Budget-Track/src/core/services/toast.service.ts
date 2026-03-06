import { Injectable, signal } from '@angular/core';

export interface Toast {
    id: number;
    message: string;
    type: 'success' | 'error' | 'warning' | 'info';
    duration?: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
    private _toasts = signal<Toast[]>([]);
    readonly toasts = this._toasts.asReadonly();
    private nextId = 0;

    success(message: string, duration = 4000): void {
        this.add(message, 'success', duration);
    }

    error(message: string, duration = 6000): void {
        this.add(message, 'error', duration);
    }

    warning(message: string, duration = 5000): void {
        this.add(message, 'warning', duration);
    }

    info(message: string, duration = 4000): void {
        this.add(message, 'info', duration);
    }

    dismiss(id: number): void {
        this._toasts.update((toasts) => toasts.filter((t) => t.id !== id));
    }

    private add(message: string, type: Toast['type'], duration: number): void {
        const id = ++this.nextId;
        this._toasts.update((toasts) => [...toasts, { id, message, type, duration }]);
        if (duration > 0) {
            setTimeout(() => this.dismiss(id), duration);
        }
    }
}
