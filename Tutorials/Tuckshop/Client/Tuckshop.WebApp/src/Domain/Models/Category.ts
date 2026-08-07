import { ModelBase, Rules, Validation } from '@singularsystems/neo-core';

export default class Category extends ModelBase {
    static typeName = "Category";

    constructor() {
        super();
        this.makeObservable();
    }

    public categoryId: number = 0;

    @Rules.Required()
    @Rules.StringLength(100)
    public categoryName: string = "";

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<Category>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        if (this.isNew || !this.categoryName) {
            return "New category";
        } else {
            return this.categoryName;
        }
    }
}