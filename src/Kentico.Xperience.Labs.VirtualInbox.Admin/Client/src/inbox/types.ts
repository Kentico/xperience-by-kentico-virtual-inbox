export type VirtualEmailListItemDto = {
  messageId: number;
  messageGuid: string;
  subject: string;
  sender: string;
  recipientsTo: string;
  sentUtc: string;
  status: string;
};

export type VirtualEmailPageDto = {
  items: VirtualEmailListItemDto[];
  totalCount: number;
  pageNumber: number;
  requestedPageNumber: number;
  pageSize: number;
  searchTerm: string;
};

export type VirtualEmailDetailDto = {
  messageGuid: string;
  subject: string;
  sender: string;
  recipientsTo: string;
  sentUtc: string;
  status: string;
  bodyHtml: string;
  bodyPlainText: string;
};
