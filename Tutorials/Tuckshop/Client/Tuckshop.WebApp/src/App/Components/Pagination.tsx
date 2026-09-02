import React from 'react';
import { observer } from 'mobx-react';
import { Neo } from '@singularsystems/neo-react';

interface IPaginationProps {
    currentPage:number;
    totalPages:number;
    onNext: () => void;
    onPrevious: () => void;
    onPageSelect: (page: number) => void;
}

@observer
export default class Pagination extends React.Component<IPaginationProps> {

    constructor(props: IPaginationProps) {
        super(props);
    }

    private getPageNumbers() : (number | "...") [] {
        const { currentPage, totalPages } = this.props;
        const pages: (number | "...")[] = [];

        // Always shows the first page, last page, current page and a couple around current.
        const siblingCount = 1;
        const startPage = Math.max(2, currentPage - siblingCount);
        const endPage = Math.min(totalPages - 1, currentPage + siblingCount);

        pages.push(1);

        if (startPage > 2) {
            pages.push("...");
        }

        for (let i = startPage; i <= endPage; i++) {
            pages.push(i);
        }

        if(endPage < totalPages - 1) {
            pages.push("...");
        }

        if(totalPages > 1) {
            pages.push(totalPages);
        }

        return pages;
    }

    public render() {
        const {currentPage, totalPages, onNext, onPrevious, onPageSelect} = this.props;
        const pageNumbers = this.getPageNumbers();

        return (
            <div className="swee-muse pagination">
                    <Neo.Button
                        className="pagination-btn"
                        icon="keyboard_double_arrow_left"
                        disabled={currentPage === 1}
                        onClick={onPrevious}
                    >
                    </Neo.Button>

                    <div className="pagination-pages">
                    {pageNumbers.map((page, index) =>
                        page === "..." ? (
                            <span key={`ellipsis-${index}`} className="pagination-ellipsis">
                                ...
                            </span>
                        ) : (
                            <button
                                key={page}
                                className={
                                    "pagination-page-btn" +
                                    (page === currentPage ? " pagination-page-active" : "")
                                }
                                onClick={() => onPageSelect(page as number)}
                                disabled={page === currentPage}
                            >
                                {page}
                            </button>
                        )
                    )}
                </div>

                    <Neo.Button
                        className="pagination-btn"
                        icon="keyboard_double_arrow_right"
                        disabled={
                            currentPage === totalPages
                        }
                        onClick={onNext}
                    >
                    </Neo.Button>
                </div>
        );
    }
}