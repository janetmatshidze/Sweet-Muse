export const LOW_STOCK_THRESHOLD = 10;

export function isLowStock(stock: number): boolean {
    return stock < LOW_STOCK_THRESHOLD;
}