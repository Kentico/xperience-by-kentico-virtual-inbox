import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card';

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

interface PreviewProps {
  selectedMessage: SimulatedEmailDetailDto | null;
  previewTab: 'email' | 'metadata';
  FormatDate: (dateString: string) => string;
  SetPreviewTab: (tab: 'email' | 'metadata') => void;
}

export const Preview = (props: PreviewProps) => (
  <Card className="shadow-lg">
    <CardHeader className="bg-gradient-to-r from-slate-50 to-slate-100">
      <CardTitle className="text-2xl !text-slate-900">Preview</CardTitle>
      {props.selectedMessage && (
        <div className="flex items-center gap-2 mt-3">
          <button
            className={`inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-medium transition-colors h-8 px-3 py-1 border ${
              props.previewTab === 'email'
                ? 'border-slate-900 bg-slate-900 !text-white'
                : 'border-slate-200 bg-white hover:bg-slate-100 !text-slate-900'
            }`}
            onClick={() => props.SetPreviewTab('email')}
            type="button"
          >
            Email
          </button>
          <button
            className={`inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-medium transition-colors h-8 px-3 py-1 border ${
              props.previewTab === 'metadata'
                ? 'border-slate-900 bg-slate-900 !text-white'
                : 'border-slate-200 bg-white hover:bg-slate-100 !text-slate-900'
            }`}
            onClick={() => props.SetPreviewTab('metadata')}
            type="button"
          >
            Metadata
          </button>
        </div>
      )}
    </CardHeader>
    <CardContent className="pt-6 space-y-4 h-full overflow-auto">
      {!props.selectedMessage && (
        <p className="text-sm !text-slate-600">
          Select an email from the inbox to view details.
        </p>
      )}

      {props.selectedMessage && (
        <>
          {props.previewTab === 'email' && (
            <div className="space-y-2 h-140">
              <iframe
                title="Simulated email HTML preview"
                sandbox=""
                srcDoc={
                  props.selectedMessage.bodyHtml || '<p>(empty html body)</p>'
                }
                className="w-full h-full border rounded"
              />
            </div>
          )}

          {props.previewTab === 'metadata' && (
            <div className="space-y-3">
              <div className="text-sm space-y-1 !text-slate-700">
                <div>
                  <span className="font-semibold !text-slate-900">
                    Subject:
                  </span>{' '}
                  {props.selectedMessage.subject || '(no subject)'}
                </div>
                <div>
                  <span className="font-semibold !text-slate-900">From:</span>{' '}
                  {props.selectedMessage.sender}
                </div>
                <div>
                  <span className="font-semibold !text-slate-900">To:</span>{' '}
                  {props.selectedMessage.recipientsTo}
                </div>
                <div>
                  <span className="font-semibold !text-slate-900">Sent:</span>{' '}
                  {props.FormatDate(props.selectedMessage.sentUtc)}
                </div>
                <div>
                  <span className="font-semibold !text-slate-900">Status:</span>{' '}
                  {props.selectedMessage.status}
                </div>
              </div>

              <div className="space-y-2">
                <h3 className="font-semibold !text-slate-900">Plain text</h3>
                <pre className="p-3 rounded bg-slate-100 text-sm whitespace-pre-wrap">
                  {props.selectedMessage.bodyPlainText ||
                    '(empty plain text body)'}
                </pre>
              </div>
            </div>
          )}
        </>
      )}
    </CardContent>
  </Card>
);
