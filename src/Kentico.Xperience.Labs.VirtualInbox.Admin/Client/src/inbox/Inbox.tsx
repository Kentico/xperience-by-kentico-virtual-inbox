import React from 'react';
import { RefreshCw, Search, Trash2 } from 'lucide-react';
import { Pagination } from '@kentico/xperience-admin-components';
import type { VirtualEmailListItemDto } from './types';
import { Button, Checkbox, Input } from '../ui/xperience';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '../ui/table';

interface InboxProps {
  emails: VirtualEmailListItemDto[];
  totalEmailCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  selectedEmailGuid: string | null;
  hasAnyEmails: boolean;
  hasSelectedEmails: boolean;
  allFilteredSelected: boolean;
  selectedEmailGuids: string[];
  searchTerm: string;
  detailInProgress: boolean;
  bulkDeleteInProgress: boolean;
  FormatDate: (dateString: string) => string;
  OnSearchTermChange: (value: string) => void;
  OnOpenEmail: (messageGuid: string) => void;
  OnDeleteAllOrSelected: () => void;
  OnToggleEmailSelection: (
    email: VirtualEmailListItemDto,
    checked: boolean,
  ) => void;
  OnToggleSelectAllFiltered: (checked: boolean) => void;
  OnPageChange: (pageNumber: number) => void;
  OnPageSizeChange: (pageSize: number) => void;
}

export const Inbox = (props: InboxProps) => (
  <div className="xp-surface h-full min-h-0 flex flex-col">
    <div className="xp-surfaceHeader">
      <div className="flex items-center justify-between gap-4">
        <h2 className="xp-subtitle">Inbox ({props.totalEmailCount})</h2>
        <Button
          className="xp-buttonCompact"
          disabled={props.bulkDeleteInProgress || !props.hasAnyEmails}
          icon={
            props.bulkDeleteInProgress ? (
              <RefreshCw className="animate-spin" size={16} />
            ) : (
              <Trash2 size={16} />
            )
          }
          inProgress={props.bulkDeleteInProgress}
          onClick={props.OnDeleteAllOrSelected}
        >
          {props.bulkDeleteInProgress
            ? 'Deleting...'
            : props.hasSelectedEmails
              ? `Delete selected (${props.selectedEmailGuids.length})`
              : 'Delete all'}
        </Button>
      </div>
      <div className="mt-3">
        <Input
          OnChange={props.OnSearchTermChange}
          placeholder="Search subject, sender, recipient"
          startIcon={<Search size={16} />}
          value={props.searchTerm}
        />
      </div>
    </div>
    <div className="flex-1 min-h-0 flex flex-col p-2">
      <div className="flex-1 min-h-0 overflow-y-auto">
        <Table className="xp-dataTable table-fixed">
          <TableHeader>
            <TableRow>
              <TableHead className="w-10">
                <Checkbox
                  ariaLabel="Select all emails on this page"
                  checked={props.allFilteredSelected}
                  OnChange={props.OnToggleSelectAllFiltered}
                />
              </TableHead>
              <TableHead>Subject</TableHead>
              <TableHead>From</TableHead>
              <TableHead className="w-60">To</TableHead>
              <TableHead>Sent</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {props.emails.map((message) => (
              <TableRow
                key={message.messageGuid}
                aria-selected={props.selectedEmailGuid === message.messageGuid}
                className={`cursor-pointer ${props.detailInProgress ? 'opacity-70' : ''}`}
                data-selected={props.selectedEmailGuid === message.messageGuid}
                onClick={() => {
                  if (!props.detailInProgress) {
                    props.OnOpenEmail(message.messageGuid);
                  }
                }}
              >
                <TableCell>
                  <Checkbox
                    ariaLabel={`Select email ${message.subject || message.messageGuid}`}
                    checked={props.selectedEmailGuids.includes(
                      message.messageGuid,
                    )}
                    OnChange={(checked) =>
                      props.OnToggleEmailSelection(message, checked)
                    }
                    onClick={(event) => event.stopPropagation()}
                  />
                </TableCell>
                <TableCell>{message.subject || '(no subject)'}</TableCell>
                <TableCell className="xp-muted">{message.sender}</TableCell>
                <TableCell className="max-w-[18rem] whitespace-normal break-all xp-muted">
                  {message.recipientsTo}
                </TableCell>
                <TableCell className="xp-muted">
                  {props.FormatDate(message.sentUtc)}
                </TableCell>
              </TableRow>
            ))}
            {props.emails.length === 0 && (
              <TableRow>
                <TableCell className="xp-emptyState" colSpan={5}>
                  {props.searchTerm.trim()
                    ? `No emails match "${props.searchTerm}".`
                    : 'No emails found.'}
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>
      <div className="flex items-center justify-between gap-4 border-t p-2">
        <span className="text-sm xp-muted">
          {props.totalEmailCount === 0
            ? '0 emails'
            : props.pageSize === 0
              ? `Showing 1-${props.totalEmailCount} of ${props.totalEmailCount}`
              : `Showing ${(props.pageNumber - 1) * props.pageSize + 1}-${Math.min(
                  props.pageNumber * props.pageSize,
                  props.totalEmailCount,
                )} of ${props.totalEmailCount}`}
        </span>
        <div className="flex items-center gap-3">
          <label className="flex items-center gap-2 text-sm xp-muted">
            Emails per page
            <select
              aria-label="Emails per page"
              className="xp-select"
              onChange={(event) =>
                props.OnPageSizeChange(Number(event.target.value))
              }
              value={props.pageSize}
            >
              <option value={50}>50</option>
              <option value={100}>100</option>
              <option value={0}>All</option>
            </select>
          </label>
          {props.totalPages > 1 && (
            <Pagination
              onPageChange={props.OnPageChange}
              selectedPage={props.pageNumber}
              totalPages={props.totalPages}
            />
          )}
        </div>
      </div>
    </div>
  </div>
);
