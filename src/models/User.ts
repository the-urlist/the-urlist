import { IClientPrincipal } from "@/models/IClientPrincipal";

export default class User {
  loggedIn: boolean = false;
  userName: string = "";

  constructor(response?: IClientPrincipal) {
    if (response) {
      this.loggedIn = true;
      this.userName = response.userDetails;
    }
  }
}
