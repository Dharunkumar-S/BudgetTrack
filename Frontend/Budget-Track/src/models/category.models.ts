export interface CategoryDto {
    categoryID: number;
    categoryName: string;
    categoryCode: string;
    isActive: boolean;
}

export interface CreateCategoryDto {
    categoryName: string;
}

export interface UpdateCategoryDto {
    categoryName: string;
    isActive: boolean;
}
