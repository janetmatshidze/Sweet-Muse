import React from 'react';
import { observer } from 'mobx-react';

interface IFooterProps {
}

@observer
export default class Footer extends React.Component<IFooterProps> {

    public render() {
        return (
            <footer
                id="footer-panel"
                className="app-footer"
            >

                <span className="footer-brand">
                    Sweet Muse
                </span>

                <span className="footer-divider">
                    •
                </span>

                <span className="footer-text">
                    Sweet moments, beautifully made.
                </span>

            </footer>
        );
    }
}