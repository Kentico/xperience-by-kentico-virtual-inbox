import React, { useEffect, useRef, useState } from 'react';
import { Info, RefreshCw } from 'lucide-react';
import { usePageCommand } from '@kentico/xperience-admin-base';
import { Inbox } from './Inbox';
import { Preview } from './Preview';
import type { VirtualEmailDetailDto, VirtualEmailListItemDto } from './types';
import { Button } from '../ui/xperience';

type LoadVirtualEmailDetailCommandParams = {
  messageGuid: string;
};

type RefreshVirtualEmailsCommandParams = {
  lastRetrievedUtc?: string;
  lastRetrievedId?: number;
};

type DeleteVirtualEmailsCommandParams = {
  messageIds?: number[];
};

interface VirtualEmailInboxClientProperties {
  isEnabled: boolean;
  emails: VirtualEmailListItemDto[];
  loadEmailsCommandName: string;
  refreshEmailsCommandName: string;
  loadEmailDetailCommandName: string;
  deleteEmailCommandName: string;
  deleteEmailsCommandName: string;
}

export const VirtualInboxTemplate = (
  props: VirtualEmailInboxClientProperties,
) => {
  const getMostRecentSentUtc = (items: VirtualEmailListItemDto[]) => {
    const newest = items.reduce<Date | null>((latest, item) => {
      const sent = new Date(item.sentUtc);

      if (Number.isNaN(sent.getTime())) {
        return latest;
      }

      if (!latest || sent > latest) {
        return sent;
      }

      return latest;
    }, null);

    return newest?.toISOString();
  };

  const getMostRecentEmailId = (items: VirtualEmailListItemDto[]) => {
    const ids = items.map((item) => item.messageId);

    if (ids.length === 0) {
      return undefined;
    }

    return Math.max(...ids);
  };

  const hasRefreshCommand =
    typeof props.refreshEmailsCommandName === 'string' &&
    props.refreshEmailsCommandName.length > 0;

  const [emails, setEmails] = useState<VirtualEmailListItemDto[]>(
    props.emails ?? [],
  );
  const [selectedEmail, setSelectedEmail] =
    useState<VirtualEmailDetailDto | null>(null);
  const [previewTab, setPreviewTab] = useState<'email' | 'metadata'>('email');
  const [searchTerm, setSearchTerm] = useState('');
  const [emailsInProgress, setEmailsInProgress] = useState(false);
  const [detailInProgress, setDetailInProgress] = useState(false);
  const [bulkDeleteInProgress, setBulkDeleteInProgress] = useState(false);
  const [selectedEmailGuids, setSelectedEmailGuids] = useState<string[]>([]);
  const [isInfoPopoverOpen, setIsInfoPopoverOpen] = useState(false);
  const infoPopoverRef = useRef<HTMLDivElement | null>(null);

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
    };
  }, []);

  const { execute: loadEmailDetail } = usePageCommand<
    VirtualEmailDetailDto | null,
    LoadVirtualEmailDetailCommandParams
  >(props.loadEmailDetailCommandName, {
    after: (response) => {
      setSelectedEmail(response ?? null);
    },
  });

  const { execute: loadEmails } = usePageCommand<VirtualEmailListItemDto[]>(
    props.loadEmailsCommandName,
    {
      after: (response) => {
        const items = response ?? [];
        setEmails(items);
      },
    },
  );

  const { execute: refreshEmails } = usePageCommand<
    VirtualEmailListItemDto[],
    RefreshVirtualEmailsCommandParams
  >(
    hasRefreshCommand
      ? props.refreshEmailsCommandName
      : props.loadEmailsCommandName,
    {
      after: (response) => {
        if (response && response.length > 0) {
          setEmails((previous) => {
            const existingGuids = new Set(
              previous.map((item) => item.messageGuid),
            );
            const onlyNew = response.filter(
              (item) => !existingGuids.has(item.messageGuid),
            );
            return [...onlyNew, ...previous];
          });
        }
      },
    },
  );

  const { execute: deleteEmails } = usePageCommand<
    number,
    DeleteVirtualEmailsCommandParams
  >(props.deleteEmailsCommandName, {
    after: () => undefined,
  });

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
    setEmailsInProgress(true);
    try {
      if (hasRefreshCommand) {
        const mostRecentSentUtc = getMostRecentSentUtc(emails);
        const mostRecentEmailId = getMostRecentEmailId(emails);
        await refreshEmails({
          lastRetrievedId: mostRecentEmailId,
          lastRetrievedUtc: mostRecentSentUtc,
        });
      } else {
        await loadEmails();
      }
    } finally {
      setEmailsInProgress(false);
    }
  };

  const removeAllOrSelectedEmails = async () => {
    setBulkDeleteInProgress(true);
    try {
      const selectedEmailIds = emails
        .filter((item) => selectedEmailGuids.includes(item.messageGuid))
        .map((item) => item.messageId);

      await deleteEmails(
        selectedEmailIds.length > 0 ? { messageIds: selectedEmailIds } : {},
      );

      if (selectedEmailIds.length > 0) {
        const selectedSet = new Set(selectedEmailGuids);
        setEmails((previous) =>
          previous.filter((item) => !selectedSet.has(item.messageGuid)),
        );
        setSelectedEmail((previous) =>
          previous && selectedSet.has(previous.messageGuid) ? null : previous,
        );
      } else {
        setEmails([]);
        setSelectedEmail(null);
      }

      setSelectedEmailGuids([]);
    } finally {
      setBulkDeleteInProgress(false);
    }
  };

  const toggleEmailSelection = (emailGuid: string, checked: boolean) => {
    if (checked) {
      setSelectedEmailGuids((previous) => {
        if (previous.includes(emailGuid)) {
          return previous;
        }

        return [...previous, emailGuid];
      });

      return;
    }

    setSelectedEmailGuids((previous) =>
      previous.filter((guid) => guid !== emailGuid),
    );
  };

  const toggleSelectAllFiltered = (checked: boolean) => {
    const filteredGuids = emails
      .filter((message) => {
        const search = searchTerm.trim().toLowerCase();

        if (!search) {
          return true;
        }

        return (
          message.subject.toLowerCase().includes(search) ||
          message.sender.toLowerCase().includes(search) ||
          message.recipientsTo.toLowerCase().includes(search) ||
          message.status.toLowerCase().includes(search)
        );
      })
      .map((item) => item.messageGuid);

    if (checked) {
      setSelectedEmailGuids((previous) =>
        Array.from(new Set([...previous, ...filteredGuids])),
      );

      return;
    }

    setSelectedEmailGuids((previous) =>
      previous.filter((guid) => !filteredGuids.includes(guid)),
    );
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

  const filteredEmails = emails.filter((email) => {
    const search = searchTerm.trim().toLowerCase();

    if (!search) {
      return true;
    }

    return (
      email.subject.toLowerCase().includes(search) ||
      email.sender.toLowerCase().includes(search) ||
      email.recipientsTo.toLowerCase().includes(search) ||
      email.status.toLowerCase().includes(search)
    );
  });

  const allFilteredSelected =
    filteredEmails.length > 0 &&
    filteredEmails.every((item) =>
      selectedEmailGuids.includes(item.messageGuid),
    );

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
            {emailsInProgress ? 'Refreshing...' : 'Refresh'}
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
                OnSearchTermChange={setSearchTerm}
                OnToggleEmailSelection={toggleEmailSelection}
                OnToggleSelectAllFiltered={toggleSelectAllFiltered}
                allFilteredSelected={allFilteredSelected}
                bulkDeleteInProgress={bulkDeleteInProgress}
                detailInProgress={detailInProgress}
                filteredEmails={filteredEmails}
                hasAnyEmails={emails.length > 0}
                hasSelectedEmails={selectedEmailGuids.length > 0}
                searchTerm={searchTerm}
                selectedEmailGuids={selectedEmailGuids}
                selectedEmailGuid={selectedEmail?.messageGuid ?? null}
                totalEmailCount={emails.length}
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
