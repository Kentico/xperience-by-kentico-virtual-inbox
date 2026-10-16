import React, { useEffect, useRef, useState } from 'react';
import { Info, RefreshCw } from 'lucide-react';
import { usePageCommand } from '@kentico/xperience-admin-base';
import { Inbox } from './Inbox';
import { Preview } from './Preview';
import type {
  VirtualEmailDetailDto,
  VirtualEmailListItemDto,
  VirtualEmailPageDto,
} from './types';
import { Button } from '../ui/xperience';

type LoadVirtualEmailDetailCommandParams = {
  messageGuid: string;
};

type LoadVirtualEmailsCommandParams = {
  pageNumber: number;
  pageSize: number;
  searchTerm: string;
};

type DeleteVirtualEmailsCommandParams = {
  messageIds?: number[];
};

interface VirtualEmailInboxClientProperties {
  isEnabled: boolean;
  emails: VirtualEmailPageDto;
  loadEmailsCommandName: string;
  refreshEmailsCommandName: string;
  loadEmailDetailCommandName: string;
  deleteEmailCommandName: string;
  deleteEmailsCommandName: string;
}

export const VirtualInboxTemplate = (
  props: VirtualEmailInboxClientProperties,
) => {
  const hasRefreshCommand =
    typeof props.refreshEmailsCommandName === 'string' &&
    props.refreshEmailsCommandName.length > 0;

  const [pageData, setPageData] = useState(props.emails);
  const [currentPage, setCurrentPage] = useState(props.emails.pageNumber);
  const [selectedEmail, setSelectedEmail] =
    useState<VirtualEmailDetailDto | null>(null);
  const [previewTab, setPreviewTab] = useState<'email' | 'metadata'>('email');
  const [searchTerm, setSearchTerm] = useState('');
  const [pageSize, setPageSize] = useState(props.emails.pageSize);
  const [emailsInProgress, setEmailsInProgress] = useState(false);
  const [detailInProgress, setDetailInProgress] = useState(false);
  const [bulkDeleteInProgress, setBulkDeleteInProgress] = useState(false);
  const [selectedEmails, setSelectedEmails] = useState<
    VirtualEmailListItemDto[]
  >([]);
  const [isInfoPopoverOpen, setIsInfoPopoverOpen] = useState(false);
  const infoPopoverRef = useRef<HTMLDivElement | null>(null);
  const latestRequestRef = useRef({
    pageNumber: props.emails.pageNumber,
    pageSize: props.emails.pageSize,
    searchTerm: props.emails.searchTerm,
  });
  const loadRequestIdRef = useRef(0);
  const searchTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const selectedEmailGuids = selectedEmails.map((email) => email.messageGuid);

  useEffect(() => {
    const handlePointerDown = (event: MouseEvent) => {
      const target = event.target as Node;

      if (!infoPopoverRef.current?.contains(target)) {
        setIsInfoPopoverOpen(false);
      }
    };

    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setIsInfoPopoverOpen(false);
      }
    };

    document.addEventListener('mousedown', handlePointerDown);
    document.addEventListener('keydown', handleEscape);

    return () => {
      document.removeEventListener('mousedown', handlePointerDown);
      document.removeEventListener('keydown', handleEscape);
      if (searchTimeoutRef.current) {
        clearTimeout(searchTimeoutRef.current);
      }
    };
  }, []);

  const applyPageResponse = (response?: VirtualEmailPageDto | null) => {
    if (
      response &&
      response.requestedPageNumber === latestRequestRef.current.pageNumber &&
      response.pageSize === latestRequestRef.current.pageSize &&
      response.searchTerm === latestRequestRef.current.searchTerm
    ) {
      setPageData(response);
      setCurrentPage(response.pageNumber);
    }
  };

  const { execute: loadEmailDetail } = usePageCommand<
    VirtualEmailDetailDto | null,
    LoadVirtualEmailDetailCommandParams
  >(props.loadEmailDetailCommandName, {
    after: (response) => {
      setSelectedEmail(response ?? null);
    },
  });

  const { execute: loadEmails } = usePageCommand<
    VirtualEmailPageDto,
    LoadVirtualEmailsCommandParams
  >(props.loadEmailsCommandName, {
    after: applyPageResponse,
  });

  const { execute: refreshEmails } = usePageCommand<
    VirtualEmailPageDto,
    LoadVirtualEmailsCommandParams
  >(
    hasRefreshCommand
      ? props.refreshEmailsCommandName
      :       props.loadEmailsCommandName,
    {
      after: applyPageResponse,
    },
  );

  const { execute: deleteEmails } = usePageCommand<
    number,
    DeleteVirtualEmailsCommandParams
  >(props.deleteEmailsCommandName, {
    after: () => undefined,
  });

  const loadPage = async (
    pageNumber: number,
    term: string,
    refresh = false,
    requestedPageSize = pageSize,
  ) => {
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
      searchTimeoutRef.current = null;
    }
    const normalizedSearchTerm = term.trim();
    const requestId = ++loadRequestIdRef.current;
    latestRequestRef.current = {
      pageNumber,
      pageSize: requestedPageSize,
      searchTerm: normalizedSearchTerm,
    };
    setCurrentPage(pageNumber);
    setPageSize(requestedPageSize);
    setEmailsInProgress(true);
    try {
      const command = refresh && hasRefreshCommand ? refreshEmails : loadEmails;
      await command({
        pageNumber,
        pageSize: requestedPageSize,
        searchTerm: normalizedSearchTerm,
      });
    } finally {
      if (requestId === loadRequestIdRef.current) {
        setEmailsInProgress(false);
      }
    }
  };

  const openEmail = async (
    emailGuid: string,
    tab: 'email' | 'metadata' = 'email',
  ) => {
    setPreviewTab(tab);
    setDetailInProgress(true);
    try {
      await loadEmailDetail({ messageGuid: emailGuid });
    } finally {
      setDetailInProgress(false);
    }
  };

  const refreshInbox = async () => {
    await loadPage(1, searchTerm, true);
  };

  const removeAllOrSelectedEmails = async () => {
    setBulkDeleteInProgress(true);
    try {
      const selectedEmailIds = selectedEmails.map((item) => item.messageId);
      const selectedGuids = new Set(selectedEmailGuids);

      await deleteEmails(
        selectedEmailIds.length > 0 ? { messageIds: selectedEmailIds } : {},
      );

      if (selectedEmailIds.length > 0) {
        setPageData((previous) => ({
          ...previous,
          items: previous.items.filter(
            (item) => !selectedGuids.has(item.messageGuid),
          ),
          totalCount: Math.max(0, previous.totalCount - selectedEmailIds.length),
        }));
        setSelectedEmail((previous) =>
          previous && selectedGuids.has(previous.messageGuid) ? null : previous,
        );
        setSelectedEmails([]);
        await loadPage(currentPage, searchTerm);
      } else {
        setPageData((previous) => ({
          ...previous,
          items: [],
          totalCount: 0,
          pageNumber: 1,
        }));
        setCurrentPage(1);
        setSelectedEmail(null);
        setSelectedEmails([]);
      }
    } finally {
      setBulkDeleteInProgress(false);
    }
  };

  const toggleEmailSelection = (
    email: VirtualEmailListItemDto,
    checked: boolean,
  ) => {
    if (checked) {
      setSelectedEmails((previous) => {
        if (previous.some((item) => item.messageGuid === email.messageGuid)) {
          return previous;
        }

        return [...previous, email];
      });

      return;
    }

    setSelectedEmails((previous) =>
      previous.filter((item) => item.messageGuid !== email.messageGuid),
    );
  };

  const toggleSelectAllFiltered = (checked: boolean) => {
    if (checked) {
      setSelectedEmails((previous) => {
        const selectedByGuid = new Map(
          previous.map((item) => [item.messageGuid, item]),
        );
        pageData.items.forEach((item) =>
          selectedByGuid.set(item.messageGuid, item),
        );
        return Array.from(selectedByGuid.values());
      });

      return;
    }

    const pageGuids = new Set(pageData.items.map((item) => item.messageGuid));
    setSelectedEmails((previous) =>
      previous.filter((item) => !pageGuids.has(item.messageGuid)),
    );
  };

  const changeSearchTerm = (value: string) => {
    setSearchTerm(value);
    setCurrentPage(1);
    latestRequestRef.current = {
      pageNumber: 1,
      pageSize,
      searchTerm: value.trim(),
    };
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }
    searchTimeoutRef.current = setTimeout(() => {
      void loadPage(1, value);
    }, 300);
  };

  const changePageSize = (value: number) => {
    void loadPage(1, searchTerm, false, value);
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);

    if (Number.isNaN(date.getTime())) {
      return dateString;
    }

    return new Intl.DateTimeFormat(undefined, {
      year: 'numeric',
      month: 'short',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    }).format(date);
  };

  const allFilteredSelected =
    pageData.items.length > 0 &&
    pageData.items.every((item) =>
      selectedEmailGuids.includes(item.messageGuid),
    );
  const totalPages =
    pageSize === 0
      ? 1
      : Math.max(1, Math.ceil(pageData.totalCount / pageSize));

  return (
    <div className="xp-page">
      <div className="w-full h-full space-y-8">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-2">
            <h1 className="xp-title">Virtual email inbox</h1>
            <div className="relative" ref={infoPopoverRef}>
              <Button
                aria-label="Virtual inbox information"
                aria-expanded={isInfoPopoverOpen}
                aria-haspopup="dialog"
                color="quinary"
                icon={<Info size={16} />}
                onClick={() => setIsInfoPopoverOpen((value) => !value)}
              />
              {isInfoPopoverOpen && (
                <div className="xp-popover">
                  When enabled, the Virtual Inbox captures emails from Xperience
                  by Kentico&apos;s email queue instead of sending them to
                  recipients. For more information, visit{' '}
                  <a
                    className="xp-link"
                    href="https://github.com/Kentico/xperience-by-kentico-virtual-inbox"
                    rel="noreferrer"
                    target="_blank"
                  >
                    Kentico&apos;s GitHub repository
                  </a>{' '}
                  for this integration.
                </div>
              )}
            </div>
          </div>
          <Button
            disabled={!props.isEnabled || emailsInProgress}
            icon={
              emailsInProgress ? (
                <RefreshCw className="animate-spin" size={16} />
              ) : undefined
            }
            inProgress={emailsInProgress}
            onClick={() => refreshInbox()}
          >
            {emailsInProgress ? 'Loading...' : 'Refresh'}
          </Button>
        </div>

        {!props.isEnabled ? (
          <div className="xp-surface">
            <div className="xp-surfaceBody p-6">
              <p className="text-base xp-muted">
                The Virtual Inbox feature is disabled through application
                settings.
              </p>
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 lg:h-[calc(100vh-300px)] lg:overflow-hidden">
            <div className="min-h-0">
              <Inbox
                FormatDate={formatDate}
                OnDeleteAllOrSelected={removeAllOrSelectedEmails}
                OnOpenEmail={openEmail}
                OnPageChange={(page) => void loadPage(page, searchTerm)}
                OnPageSizeChange={changePageSize}
                OnSearchTermChange={changeSearchTerm}
                OnToggleEmailSelection={toggleEmailSelection}
                OnToggleSelectAllFiltered={toggleSelectAllFiltered}
                allFilteredSelected={allFilteredSelected}
                bulkDeleteInProgress={bulkDeleteInProgress}
                detailInProgress={detailInProgress}
                emails={pageData.items}
                hasAnyEmails={pageData.totalCount > 0 || !!searchTerm.trim()}
                hasSelectedEmails={selectedEmailGuids.length > 0}
                pageNumber={currentPage}
                pageSize={pageSize}
                searchTerm={searchTerm}
                selectedEmailGuids={selectedEmailGuids}
                selectedEmailGuid={selectedEmail?.messageGuid ?? null}
                totalEmailCount={pageData.totalCount}
                totalPages={totalPages}
              />
            </div>
            <div className="min-h-0">
              <Preview
                FormatDate={formatDate}
                OnClose={() => setSelectedEmail(null)}
                SetPreviewTab={setPreviewTab}
                previewTab={previewTab}
                selectedEmail={selectedEmail}
              />
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
