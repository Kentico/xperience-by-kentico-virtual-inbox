import React, { useState } from 'react';
import { usePageCommand } from '@kentico/xperience-admin-base';
import { Inbox } from './Inbox';
import { Preview } from './Preview';

type SimulatedEmailListItemDto = {
  messageGuid: string;
  subject: string;
  sender: string;
  recipientsTo: string;
  sentUtc: string;
  status: string;
};

type SimulatedEmailDetailDto = {
  messageGuid: string;
  subject: string;
  sender: string;
  recipientsTo: string;
  sentUtc: string;
  status: string;
  bodyHtml: string;
  bodyPlainText: string;
};

type LoadSimulatedEmailDetailCommandParams = {
  messageGuid: string;
};

type RefreshSimulatedEmailsCommandParams = {
  lastRetrievedUtc?: string;
};

type DeleteSimulatedEmailCommandParams = {
  messageGuid: string;
};

interface SimulatedEmailInboxClientProperties {
  messages: SimulatedEmailListItemDto[];
  loadMessagesCommandName: string;
  refreshMessagesCommandName: string;
  loadMessageDetailCommandName: string;
  deleteMessageCommandName: string;
}

export const VirtualInboxTemplate = (
  props: SimulatedEmailInboxClientProperties,
) => {
  const hasRefreshCommand =
    typeof props.refreshMessagesCommandName === 'string' &&
    props.refreshMessagesCommandName.length > 0;

  const [messages, setMessages] = useState<SimulatedEmailListItemDto[]>(
    props.messages ?? [],
  );
  const [selectedMessage, setSelectedMessage] =
    useState<SimulatedEmailDetailDto | null>(null);
  const [previewTab, setPreviewTab] = useState<'email' | 'metadata'>('email');
  const [searchTerm, setSearchTerm] = useState('');
  const [lastRetrievedUtc, setLastRetrievedUtc] = useState<string | undefined>(
    new Date().toISOString(),
  );
  const [messagesInProgress, setMessagesInProgress] = useState(false);
  const [detailInProgress, setDetailInProgress] = useState(false);
  const [deleteInProgressGuid, setDeleteInProgressGuid] = useState<
    string | null
  >(null);

  const { execute: loadMessageDetail } = usePageCommand<
    SimulatedEmailDetailDto | null,
    LoadSimulatedEmailDetailCommandParams
  >(props.loadMessageDetailCommandName, {
    after: (response) => {
      setSelectedMessage(response ?? null);
    },
  });

  const { execute: loadMessages } = usePageCommand<SimulatedEmailListItemDto[]>(
    props.loadMessagesCommandName,
    {
      after: (response) => {
        if (response) {
          setMessages(response);
        }
        setLastRetrievedUtc(new Date().toISOString());
      },
    },
  );

  const { execute: refreshMessages } = usePageCommand<
    SimulatedEmailListItemDto[],
    RefreshSimulatedEmailsCommandParams
  >(
    hasRefreshCommand
      ? props.refreshMessagesCommandName
      : props.loadMessagesCommandName,
    {
      after: (response) => {
        if (response && response.length > 0) {
          setMessages((previous) => {
            const existingGuids = new Set(
              previous.map((item) => item.messageGuid),
            );
            const onlyNew = response.filter(
              (item) => !existingGuids.has(item.messageGuid),
            );
            return [...onlyNew, ...previous];
          });
        }
        setLastRetrievedUtc(new Date().toISOString());
      },
    },
  );

  const { execute: deleteMessage } = usePageCommand<
    boolean,
    DeleteSimulatedEmailCommandParams
  >(props.deleteMessageCommandName, {
    after: () => undefined,
  });

  const openMessage = async (
    messageGuid: string,
    tab: 'email' | 'metadata' = 'email',
  ) => {
    setPreviewTab(tab);
    setDetailInProgress(true);
    try {
      await loadMessageDetail({ messageGuid });
    } finally {
      setDetailInProgress(false);
    }
  };

  const refreshInbox = async () => {
    setMessagesInProgress(true);
    try {
      if (hasRefreshCommand) {
        await refreshMessages({ lastRetrievedUtc });
      } else {
        await loadMessages();
      }
    } finally {
      setMessagesInProgress(false);
    }
  };

  const removeMessage = async (messageGuid: string) => {
    setDeleteInProgressGuid(messageGuid);
    try {
      await deleteMessage({ messageGuid });
      setMessages((previous) =>
        previous.filter((item) => item.messageGuid !== messageGuid),
      );
      setSelectedMessage((previous) =>
        previous?.messageGuid === messageGuid ? null : previous,
      );
    } finally {
      setDeleteInProgressGuid(null);
    }
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

  const filteredMessages = messages.filter((message) => {
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
  });

  const buttonClassName =
    'inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400 disabled:pointer-events-none disabled:opacity-50 h-9 px-4 py-2 border border-slate-200 bg-white hover:bg-slate-100 !text-slate-900';

  return (
    <div className="min-h-[calc(100vh-160px)] bg-gradient-to-br from-slate-50 to-slate-100 p-4 pb-0">
      <div className="w-full h-full space-y-8">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h1 className="text-4xl font-bold tracking-tight !text-slate-900">
              Simulated email inbox
            </h1>
            <p className="text-lg !text-slate-600">
              Emails captured by custom simulated email client.
            </p>
          </div>
          <button
            className={buttonClassName}
            disabled={messagesInProgress}
            onClick={() => refreshInbox()}
            type="button"
          >
            {messagesInProgress ? 'Refreshing...' : 'Refresh'}
          </button>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 lg:min-h-[calc(100vh-300px)]">
          <Inbox
            FormatDate={formatDate}
            OnOpenMessage={openMessage}
            OnRemoveMessage={removeMessage}
            OnSearchTermChange={setSearchTerm}
            deleteInProgressGuid={deleteInProgressGuid}
            detailInProgress={detailInProgress}
            filteredMessages={filteredMessages}
            searchTerm={searchTerm}
          />
          <Preview
            FormatDate={formatDate}
            SetPreviewTab={setPreviewTab}
            previewTab={previewTab}
            selectedMessage={selectedMessage}
          />
        </div>
      </div>
    </div>
  );
};
