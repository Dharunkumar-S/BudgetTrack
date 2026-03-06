import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';
import { AuditLogDto } from '@models/audit.models';
import { PagedResult } from '@models/pagination.models';

@Injectable({ providedIn: 'root' })
export class AuditService {
    private http = inject(HttpClient);
    private apiUrl = environment.apiUrl;

    getAuditLogs(
        pageNumber = 1,
        pageSize = 10,
        search?: string,
        action?: string,
        entityType?: string
    ): Observable<PagedResult<AuditLogDto>> {
        let params = new HttpParams()
            .set('pageNumber', pageNumber.toString())
            .set('pageSize', pageSize.toString());
        if (search) params = params.set('search', search);
        if (action) params = params.set('action', action);
        if (entityType) params = params.set('entityType', entityType);
        return this.http.get<PagedResult<AuditLogDto>>(`${this.apiUrl}/api/audits`, { params });
    }

    getAuditLogsByUser(userId: number): Observable<AuditLogDto[]> {
        return this.http.get<AuditLogDto[]>(`${this.apiUrl}/api/audits/${userId}`);
    }
}
