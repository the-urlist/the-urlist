export default {
  AUTH_URL: function(provider: string) {
    return `/.auth/login/${provider}`;
  },
  LOGOUT_URL: `/.auth/logout?post_logout_redirect_uri=`,
  FUNCTION_KEY: process.env.VUE_APP_FUNCTION_KEY,
  SITE_URL: process.env.VUE_APP_SITE_URL
};
