import React from 'react';
import { observer } from 'mobx-react';
import { Neo } from '@singularsystems/neo-react';

interface IPaginationProps {
    currentPage:number;
    totalPages:number;
    onNext: () => void;
    onPrevious:() => void;
}

@observer
export default class Pagination extends React.Component<IPaginationProps> {

    constructor(props: IPaginationProps) {
        super(props);
    }

    public render() {
        const {currentPage, totalPages, onNext, onPrevious} = this.props
        return (
            <div className="swee-muse pagination">
                    <Neo.Button
                        className="pagination-btn"
                        icon="keyboard_double_arrow_left"
                        disabled={currentPage === 1}
                        onClick={onPrevious}
                    >
                    </Neo.Button>

                    <span className="pagination-info">
                        Page {currentPage} of {totalPages}
                    </span>

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