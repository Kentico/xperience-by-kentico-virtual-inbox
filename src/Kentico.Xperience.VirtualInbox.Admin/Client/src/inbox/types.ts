export type VirtualEmailListItemDto = {
  messageId: number;
  messageGuid: string;
  subject: string;
  sender: string;
  recipientsTo: string;
  sentUtc: string;
  status: string;
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
