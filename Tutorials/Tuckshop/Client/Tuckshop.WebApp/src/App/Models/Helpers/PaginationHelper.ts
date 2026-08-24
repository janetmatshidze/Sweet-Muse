
import { observable, computed, action, makeObservable } from 'mobx';

export default class PaginationHelper<T> {
    constructor(
        private getItems: () => T[],
        public pageSize: number = 6) {

        makeObservable(this);
    }

    @observable
    public currentPage: number = 1;

    @computed
    public get totalPages() {
        return Math.max(1, Math.ceil(this.getItems().length / this.pageSize));
    }

    @computed
    public get pagedItems() {
        const start = (this.currentPage - 1) * this.pageSize;
        return this.getItems().slice(start, start + this.pageSize);
    }

    @action
    public goToPage(page: number) {
        if (page < 1 || page > this.totalPages) {

            return;
        }

        this.currentPage = page;
    }

    @action
    public nextPage() {

        this.goToPage(this.currentPage + 1);
    }

    @action
    public previousPage() {

        this.goToPage(this.currentPage - 1);
    }

    @action
    public clamp() {
        
        if (this.currentPage > this.totalPages) {

            this.currentPage = this.totalPages;
        }
    }

    @action
    public reset() {
        this.currentPage = 1;
    }
}