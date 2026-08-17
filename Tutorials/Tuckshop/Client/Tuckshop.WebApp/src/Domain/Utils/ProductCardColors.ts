
  export function getProductColorClass(categoryId: number): string {
     const index = categoryId % 6;
    return `product-color-${index}`;
  }
