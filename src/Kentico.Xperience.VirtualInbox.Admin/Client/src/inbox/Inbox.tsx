import React from 'react';
import { Eye, FileText, RefreshCw, Search, Trash2 } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card';
import type { VirtualEmailListItemDto } from './types';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '../ui/table';

interface InboxProps {
  filteredEmails: VirtualEmailListItemDto[];
  hasAnyEmails: boolean;
  hasSelectedEmails: boolean;
  allFilteredSelected: boolean;
  selectedEmailGuids: string[];
  searchTerm: string;
  detailInProgress: boolean;
  bulkDeleteInProgress: boolean;
  deleteInProgressEmailGuid: string | null;
  FormatDate: (dateString: string) => string;
  OnSearchTermChange: (value: string) => void;
  OnOpenEmail: (messageGuid: string, tab?: 'email' | 'metadata') => void;
  OnRemoveEmail: (messageGuid: string) => void;
  OnDeleteAllOrSelected: () => void;
  OnToggleEmailSelection: (messageGuid: string, checked: boolean) => void;
  OnToggleSelectAllFiltered: (checked: boolean) => void;
}

export const Inbox = (props: InboxProps) => (
  <Card className="shadow-lg overflow-hidden">
    <CardHeader className="bg-gradient-to-r from-slate-50 to-slate-100">
      <div className="flex items-center justify-between gap-4">
        <CardTitle className="text-2xl !text-slate-900">Inbox</CardTitle>
        <button
          className="inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-md text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400 disabled:pointer-events-none disabled:opacity-50 h-9 px-4 py-2 border border-slate-200 bg-white hover:bg-slate-100 !text-slate-900"
          disabled={props.bulkDeleteInProgress || !props.hasAnyEmails}
          onClick={props.OnDeleteAllOrSelected}
          type="button"
        >
          {props.bulkDeleteInProgress ? (
            <RefreshCw className="animate-spin" size={16} />
          ) : (
            <Trash2 size={16} />
          )}
          {props.bulkDeleteInProgress
            ? 'Deleting...'
            : props.hasSelectedEmails
              ? 'Delete selected'
              : 'Delete all'}
        </button>
      </div>
      <div className="relative mt-3">
        <Search
          className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500"
          size={16}
        />
        <input
          className="w-full rounded-md border border-slate-300 bg-white py-2 pl-9 pr-3 text-sm !text-slate-900 focus:border-slate-400 focus:outline-none"
          onChange={(event) => props.OnSearchTermChange(event.target.value)}
          placeholder="Search subject, sender, recipient"
          type="text"
          value={props.searchTerm}
        />
      </div>
    </CardHeader>
    <CardContent className="p-0 overflow-auto">
      <Table className="table-fixed">
        <TableHeader>
          <TableRow className="bg-slate-50">
            <TableHead className="w-10 font-semibold !text-slate-700">
              <input
                aria-label="Select all filtered emails"
                checked={props.allFilteredSelected}
                className="h-4 w-4 rounded border-slate-300"
                onChange={(event) =>
                  props.OnToggleSelectAllFiltered(event.target.checked)
                }
                type="checkbox"
              />
            </TableHead>
            <TableHead className="font-semibold !text-slate-700">
              Subject
            </TableHead>
            <TableHead className="font-semibold !text-slate-700">
              From
            </TableHead>
            <TableHead className="w-60 font-semibold !text-slate-700">
              To
            </TableHead>
            <TableHead className="font-semibold !text-slate-700">
              Sent
            </TableHead>
            <TableHead className="w-16 font-semibold !text-slate-700">
              Open
            </TableHead>
            <TableHead className="w-20 font-semibold !text-slate-700">
              Metadata
            </TableHead>
            <TableHead className="w-16 font-semibold !text-slate-700">
              Delete
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {props.filteredEmails.map((message) => (
            <TableRow key={message.messageGuid}>
              <TableCell>
                <input
                  aria-label={`Select email ${message.subject || message.messageGuid}`}
                  checked={props.selectedEmailGuids.includes(
                    message.messageGuid,
                  )}
                  className="h-4 w-4 rounded border-slate-300"
                  onChange={(event) =>
                    props.OnToggleEmailSelection(
                      message.messageGuid,
                      event.target.checked,
                    )
                  }
                  type="checkbox"
                />
              </TableCell>
              <TableCell className="!text-slate-900">
                {message.subject || '(no subject)'}
              </TableCell>
              <TableCell className="!text-slate-700">
                {message.sender}
              </TableCell>
              <TableCell className="max-w-[18rem] whitespace-normal break-all !text-slate-700">
                {message.recipientsTo}
              </TableCell>
              <TableCell className="!text-slate-700">
                {props.FormatDate(message.sentUtc)}
              </TableCell>
              <TableCell>
                <button
                  className="inline-flex items-center justify-center rounded-md h-9 w-9 border border-slate-200 bg-white hover:bg-slate-100 !text-slate-900"
                  disabled={props.detailInProgress}
                  onClick={() => props.OnOpenEmail(message.messageGuid)}
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
                    props.OnOpenEmail(message.messageGuid, 'metadata')
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
                  disabled={
                    props.deleteInProgressEmailGuid === message.messageGuid
                  }
                  onClick={() => props.OnRemoveEmail(message.messageGuid)}
                  title="Delete email"
                  type="button"
                >
                  {props.deleteInProgressEmailGuid === message.messageGuid ? (
                    <RefreshCw className="animate-spin" size={16} />
                  ) : (
                    <Trash2 size={16} />
                  )}
                </button>
              </TableCell>
            </TableRow>
          ))}
          {props.filteredEmails.length === 0 && (
            <TableRow>
              <TableCell
                className="p-8 text-center !text-slate-500"
                colSpan={8}
              >
                {props.searchTerm.trim()
                  ? `No emails match "${props.searchTerm}".`
                  : 'No emails found.'}
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </CardContent>
  </Card>
);
