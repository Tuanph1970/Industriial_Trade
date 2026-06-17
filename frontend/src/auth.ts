import { UserManager, WebStorageStateStore, type UserManagerSettings } from 'oidc-client-ts';

const settings: UserManagerSettings = {
  authority: import.meta.env.VITE_KEYCLOAK_AUTHORITY ?? 'http://localhost:8090/realms/industry-trade',
  client_id: import.meta.env.VITE_KEYCLOAK_CLIENT_ID ?? 'industry-trade-spa',
  redirect_uri: window.location.origin,
  post_logout_redirect_uri: window.location.origin,
  response_type: 'code', // Authorization Code + PKCE
  scope: 'openid profile',
  userStore: new WebStorageStateStore({ store: window.sessionStorage }),
};

export const userManager = new UserManager(settings);

// Strip the ?code&state from the URL after a successful login.
export function onSigninCallback() {
  window.history.replaceState({}, document.title, window.location.pathname);
}
