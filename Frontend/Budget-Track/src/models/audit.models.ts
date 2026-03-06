export interface AuditLogDto {
    auditLogID: number;
    userID: number;
    userName: string;
    employeeId: string | null;   // replaces entityID display
    entityType: string;          // module name (Budget, Expense, etc.)
    entityID: number;
    action: string;
    oldValue: Record<string, unknown> | null;
    newValue: Record<string, unknown> | null;
    timestamp: string;
    notes: string;
}

export interface AuditListParams {
    pageNumber?: number;
    pageSize?: number;
    employeeId?: string;
    action?: string;
    entityType?: string;
}
