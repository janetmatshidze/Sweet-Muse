import React from 'react';
import { observer } from 'mobx-react';
import { Neo } from '@singularsystems/neo-react';

interface IFilterOption {
    label: string;
    value: string | number | null;
}
interface IFilterOptionGroupProps {
    options: IFilterOption[];
    selectedValue: string | number | null;
    onSelect:(value: any) => void;
}

@observer
export default class FilterOptionGroup extends React.Component<IFilterOptionGroupProps> {

    constructor(props: IFilterOptionGroupProps) {
        super(props);
    }

    public render() {
        const { options, selectedValue, onSelect } = this.props;
        return (
            <>
            {options.map(option => (
                <Neo.Button
                    key={String(option.value)}
                    type="button"
                    className={"filter-option" + (selectedValue === option.value ? " active" : "")}
                    onClick={() => onSelect(option.value)}
                >
                    {option.label}
                </Neo.Button>
            ))}
            </>
        );
    }
}