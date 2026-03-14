import React from 'react';
import { RefreshCw, Search, Trash2 } from 'lucide-react';
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
  filteredEmails: VirtualEmailListItemDto[];
  totalEmailCount: number;
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
  OnToggleEmailSelection: (messageGuid: string, checked: boolean) => void;
  OnToggleSelectAllFiltered: (checked: boolean) => void;
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
              : `Delete all (${props.selectedEmailGuids.length} selected)`}
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
    <div className="flex-1 min-h-0 overflow-y-auto p-2">
      <Table className="xp-dataTable table-fixed">
        <TableHeader>
          <TableRow>
            <TableHead className="w-10">
              <Checkbox
                ariaLabel="Select all filtered emails"
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
          {props.filteredEmails.map((message) => (
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
                    props.OnToggleEmailSelection(message.messageGuid, checked)
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
          {props.filteredEmails.length === 0 && (
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
  </div>
);
