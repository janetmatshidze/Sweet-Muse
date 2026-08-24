import React from 'react';
import { observer } from 'mobx-react';
import { Neo } from '@singularsystems/neo-react';

interface ISearchableSelectProps<T> {
    items: T[];
    valueMember: keyof T;
    displayMember: keyof T;
    value: any;
    placeholder?: string;
    nullText?: string;
    onSelect: (item: T | null) => void;
}

@observer
export default class SearchableSelect<T> extends React.Component<ISearchableSelectProps<T>> {

    public state = { searchTerm: '', isOpen: false };

    private get filteredItems() {
        const { items, displayMember, searchMember } = this.props as any;
        const term = this.state.searchTerm.trim().toLowerCase();

        if (!term) return items;

        return items.filter((item: T) =>
            String(item[this.props.displayMember]).toLowerCase().includes(term)
        );
    }

    private selectItem = (item: T | null) => {
        this.props.onSelect(item);
        this.setState({ isOpen: false, searchTerm: '' });
    };

    public render() {
        const { items, valueMember, displayMember, value, placeholder, nullText } = this.props;
        const selected = items.find(i => i[valueMember] === value);

        return (
            <div className="searchable-select">
                <Neo.Button
                    type="button"
                    className="searchable-select-toggle"
                    onClick={() => this.setState({ isOpen: !this.state.isOpen })}
                >
                    {selected ? String(selected[displayMember]) : (nullText ?? placeholder ?? 'Select...')}
                </Neo.Button>

                {this.state.isOpen && (
                    <div className="searchable-select-dropdown">
                        <input
                            autoFocus
                            type="text"
                            className="searchable-select-input"
                            placeholder={placeholder ?? 'Search...'}
                            value={this.state.searchTerm}
                            onChange={(e) => this.setState({ searchTerm: e.target.value })}
                        />

                        <div className="searchable-select-list">
                            {nullText && (
                                <div className="searchable-select-option" onClick={() => this.selectItem(null)}>
                                    {nullText}
                                </div>
                            )}

                            {this.filteredItems.map((item: T) => (
                                <div
                                    key={String(item[valueMember])}
                                    className="searchable-select-option"
                                    onClick={() => this.selectItem(item)}
                                >
                                    {String(item[displayMember])}
                                </div>
                            ))}

                            {this.filteredItems.length === 0 && (
                                <div className="searchable-select-empty">No results</div>
                            )}
                        </div>
                    </div>
                )}
            </div>
        );
    }
}