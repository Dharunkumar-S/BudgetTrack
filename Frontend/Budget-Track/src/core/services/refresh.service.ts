import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class RefreshService {
    private subject = new Subject<void>();
    private completeSubject = new Subject<boolean>();
    private pending = false;

    readonly refresh$ = this.subject.asObservable();
    readonly refreshComplete$ = this.completeSubject.asObservable();

    notifyRefresh(): void {
        this.pending = true;
        this.subject.next();
    }

    notifyComplete(success = true): void {
        if (!this.pending) return;
        this.pending = false;
        this.completeSubject.next(success);
    }
}
