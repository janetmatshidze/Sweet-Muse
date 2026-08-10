import React from 'react';
import { observer } from 'mobx-react';

interface IFooterProps {
    
}

@observer
export default class Footer extends React.Component<IFooterProps> {

    constructor(props: IFooterProps) {
        super(props);
    }

    public render() {
        return (
            <div>
            </div>
        );
    }
}