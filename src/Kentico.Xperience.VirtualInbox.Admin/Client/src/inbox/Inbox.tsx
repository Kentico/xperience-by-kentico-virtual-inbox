import React from 'react';
import { Eye, FileText, RefreshCw, Search, Trash2 } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '../ui/table';

interface SimulatedEmailListItemDto {
  messageGuid: string;
  subject: string;
  sender: string;
  recipientsTo: string;
  sentUtc: string;
  status: string;
}

interface InboxProps {
  filteredMessages: SimulatedEmailListItemDto[];
  searchTerm: string;
  detailInProgress: boolean;
  deleteInProgressGuid: string | null;
  FormatDate: (dateString: string) => string;
  OnSearchTermChange: (value: string) => void;
  OnOpenMessage: (messageGuid: string, tab?: 'email' | 'metadata') => void;
  OnRemoveMessage: (messageGuid: string) => void;
}

export const Inbox = (props: InboxProps) => (
  <Card className="shadow-lg overflow-hidden">
    <CardHeader className="bg-gradient-to-r from-slate-50 to-slate-100">
      <CardTitle className="text-2xl !text-slate-900">Inbox</CardTitle>
      <div className="relative mt-3">
        <Search
          className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500"
          size={16}
        />
        <input
          className="w-full rounded-md border border-slate-300 bg-white py-2 pl-9 pr-3 text-sm !text-slate-900 focus:border-slate-400 focus:outline-none"
          onChange={(event) => props.OnSearchTermChange(event.target.value)}
          placeholder="Search subject, sender, recipient, status"
          type="text"
          value={props.searchTerm}
        />
      </div>
    </CardHeader>
    <CardContent className="p-0 overflow-auto">
      <Table>
        <TableHeader>
          <TableRow className="bg-slate-50">
            <TableHead className="font-semibold !text-slate-700">
              Subject
            </TableHead>
            <TableHead className="font-semibold !text-slate-700">
              From
            </TableHead>
            <TableHead className="font-semibold !text-slate-700">To</TableHead>
            <TableHead className="font-semibold !text-slate-700">
              Sent
            </TableHead>
            <TableHead className="font-semibold !text-slate-700">
              Status
            </TableHead>
            <TableHead className="font-semibold !text-slate-700">
              Open
            </TableHead>
            <TableHead className="font-semibold !text-slate-700">
              Metadata
            </TableHead>
            <TableHead className="font-semibold !text-slate-700">
              Delete
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {props.filteredMessages.map((message) => (
            <TableRow key={message.messageGuid}>
              <TableCell className="!text-slate-900">
                {message.subject || '(no subject)'}
              </TableCell>
              <TableCell className="!text-slate-700">
                {message.sender}
              </TableCell>
              <TableCell className="!text-slate-700">
                {message.recipientsTo}
              </TableCell>
              <TableCell className="!text-slate-700">
                {props.FormatDate(message.sentUtc)}
              </TableCell>
              <TableCell className="!text-slate-700">
                {message.status}
              </TableCell>
              <TableCell>
                <button
                  className="inline-flex items-center justify-center rounded-md h-9 w-9 border border-slate-200 bg-white hover:bg-slate-100 !text-slate-900"
                  disabled={props.detailInProgress}
                  onClick={() => props.OnOpenMessage(message.messageGuid)}
                  title="Open email"
                  type="button"
                >
                  {props.detailInProgress ? (
                    <RefreshCw className="animate-spin" size={16} />
                  ) : (
                    <Eye size={16} />
                  )}
                </button>
              </TableCell>
              <TableCell>
                <button
                  className="inline-flex items-center justify-center rounded-md h-9 w-9 border border-slate-200 bg-white hover:bg-slate-100 !text-slate-900"
                  disabled={props.detailInProgress}
                  onClick={() =>
                    props.OnOpenMessage(message.messageGuid, 'metadata')
                  }
                  title="Open metadata"
                  type="button"
                >
                  {props.detailInProgress ? (
                    <RefreshCw className="animate-spin" size={16} />
                  ) : (
                    <FileText size={16} />
                  )}
                </button>
              </TableCell>
              <TableCell>
                <button
                  className="inline-flex items-center justify-center rounded-md h-9 w-9 border border-slate-200 bg-white hover:bg-slate-100 !text-slate-900 disabled:opacity-50"
                  disabled={props.deleteInProgressGuid === message.messageGuid}
                  onClick={() => props.OnRemoveMessage(message.messageGuid)}
                  title="Delete email"
                  type="button"
                >
                  {props.deleteInProgressGuid === message.messageGuid ? (
                    <RefreshCw className="animate-spin" size={16} />
                  ) : (
                    <Trash2 size={16} />
                  )}
                </button>
              </TableCell>
            </TableRow>
          ))}
          {props.filteredMessages.length === 0 && (
            <TableRow>
              <TableCell
                className="p-8 text-center !text-slate-500"
                colSpan={8}
              >
                {props.searchTerm.trim()
                  ? `No emails match "${props.searchTerm}".`
                  : 'No simulated emails found.'}
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </CardContent>
  </Card>
);
