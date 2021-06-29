interface IClientPrincipal {
  userId: string;
  userRoles: Array<string>;
  userDetails: string;
}

export { IClientPrincipal };
