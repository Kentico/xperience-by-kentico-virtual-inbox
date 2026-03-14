import React from 'react';
import { X } from 'lucide-react';
import type { VirtualEmailDetailDto } from './types';
import { Button, NameToggleButtons } from '../ui/xperience';

interface PreviewProps {
  selectedEmail: VirtualEmailDetailDto | null;
  previewTab: 'email' | 'metadata';
  FormatDate: (dateString: string) => string;
  SetPreviewTab: (tab: 'email' | 'metadata') => void;
  OnClose: () => void;
}

export const Preview = (props: PreviewProps) => (
  <div className="xp-surface h-full min-h-0 flex flex-col">
    <div className="xp-surfaceHeader">
      <div className="flex items-center justify-between">
        <h2 className="xp-subtitle">Preview</h2>
        {props.selectedEmail && (
          <Button
            color="quinary"
            icon={<X size={16} />}
            onClick={props.OnClose}
            title="Close preview"
          />
        )}
      </div>
      {props.selectedEmail && (
        <NameToggleButtons
          items={[
            { id: 'email', label: 'Email' },
            { id: 'metadata', label: 'Metadata' },
          ]}
          OnChange={(id) => props.SetPreviewTab(id as 'email' | 'metadata')}
          selectedItemId={props.previewTab}
        />
      )}
    </div>
    <div
      className={`xp-surfaceBody xp-previewBody flex-1 min-h-0 ${props.selectedEmail ? 'overflow-hidden' : 'overflow-auto'}`}
    >
      {!props.selectedEmail && (
        <p className="text-sm xp-muted">
          Select an email from the inbox to view details.
        </p>
      )}

      {props.selectedEmail && (
        <>
          {props.previewTab === 'email' && (
            <div className="xp-emailPane">
              <iframe
                title="Virtual email HTML preview"
                sandbox=""
                srcDoc={
                  props.selectedEmail.bodyHtml || '<p>(empty html body)</p>'
                }
                className="xp-emailFrame"
              />
            </div>
          )}

          {props.previewTab === 'metadata' && (
            <div className="xp-metadataPane">
              <div className="xp-metaGrid xp-muted">
                <div className="xp-metaRow">
                  <span className="xp-metaLabel">Subject:</span>
                  <span>{props.selectedEmail.subject || '(no subject)'}</span>
                </div>
                <div className="xp-metaRow">
                  <span className="xp-metaLabel">From:</span>
                  <span>{props.selectedEmail.sender}</span>
                </div>
                <div className="xp-metaRow">
                  <span className="xp-metaLabel">To:</span>
                  <span>{props.selectedEmail.recipientsTo}</span>
                </div>
                <div className="xp-metaRow">
                  <span className="xp-metaLabel">Sent:</span>
                  <span>{props.FormatDate(props.selectedEmail.sentUtc)}</span>
                </div>
                <div className="xp-metaRow">
                  <span className="xp-metaLabel">Status:</span>
                  <span>{props.selectedEmail.status}</span>
                </div>
              </div>

              <div className="xp-plainTextPane">
                <h3 className="xp-sectionTitle">Plain text</h3>
                <pre className="xp-codeBlock">
                  {props.selectedEmail.bodyPlainText ||
                    '(empty plain text body)'}
                </pre>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  </div>
);
