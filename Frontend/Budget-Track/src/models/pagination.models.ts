// Pagination wrapper model used by all list endpoints
export interface PagedResult<T> {
    data: T[];
    pageNumber: number;
    pageSize: number;
    totalRecords: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
    firstPage: number;
    lastPage: number;
    nextPage: number | null;
    previousPage: number | null;
    firstItemIndex: number;
    lastItemIndex: number;
    currentPageItemCount: number;
    isFirstPage: boolean;
    isLastPage: boolean;
}

export interface PaginationParams {
    pageNumber: number;
    pageSize: number;
}

export interface SortParams {
    sortBy?: string;
    sortOrder?: 'asc' | 'desc';
}

export interface ApiError {
    success: boolean;
    message: string;
    errors?: Record<string, string[]>;
    status?: number;
}
