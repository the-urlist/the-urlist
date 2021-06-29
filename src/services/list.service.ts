import ApiService from "./api.service";
import ILink from "@/models/ILink";
import IOGData from "@/models/IOGData";
import Link from "@/models/Link";
import List from "@/models/List";
import config from "@/config";

const ListService = {
  async get(vanityUrl: string): Promise<List> {
    const response = await ApiService.get(`/api/links/${vanityUrl}`);
    return new List(vanityUrl, response.data.description, <Array<ILink>>(
      response.data.links
    ));
  },
  async create(payload: object): Promise<string> {
    const response = await ApiService.post(`/api/links`, payload);
    return response.data.vanityUrl;
  },
  async validate(url: string, id: string): Promise<ILink> {
    const response = await ApiService.post(`/api/validatePage`, {
      url: url,
      id: id
    });
    const ogData = <IOGData>response.data;

    return new Link(
      id,
      url,
      ogData.title,
      ogData.description,
      ogData.image ? ogData.image.replace(/(^\w+:|^)/, "") : ""
    );
  },
  update(vanityUrl: string, payload: object) {
    return ApiService.patch(`/api/links/${vanityUrl}`, payload);
  },
  destroy(vanityUrl: string) {
    return ApiService.destroy(`/api/links/${vanityUrl}`);
  }
};

export default ListService;
