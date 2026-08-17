const categoryColorCount = 6;

export function getCategoryColorClass(categoryId: number): string {
    const index = categoryId % categoryColorCount;
    return `category-pill category-color-${index}`;
}