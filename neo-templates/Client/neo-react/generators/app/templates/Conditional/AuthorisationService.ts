import { injectable } from 'inversify';

/**
 * If you need actual authorisation, use the @singularsystems/neo-authorisation package.
 */
@injectable()
export default class AuthorisationService {
    public hasRole(roleName: string) : boolean {
        return true;
    }
    public loadRoles() : Promise<void> {
        return Promise.resolve();
    }
}