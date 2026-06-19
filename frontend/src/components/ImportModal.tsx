import { useMemo, useState } from 'react';
import { App as AntApp, Alert, Button, Modal, Space, Table, Tag, Tooltip, Typography, Upload } from 'antd';
import { InboxOutlined } from '@ant-design/icons';
import { BulkImportResult, ImportParseResult, parseImportFile } from '../api/client';

/** Case/space-insensitive cell lookup by header — tolerant of how users label columns. */
export const getCell = (cells: Record<string, string>, header: string): string => {
  const key = Object.keys(cells).find((k) => k.trim().toLowerCase() === header.trim().toLowerCase());
  return key ? (cells[key] ?? '').trim() : '';
};

/** Parses an enum cell that may be a numeric code or a Vietnamese/English label. */
export const parseEnum = (raw: string, labels: Record<number, string>): number | undefined => {
  if (!raw) return undefined;
  const asNum = Number(raw);
  if (!Number.isNaN(asNum) && labels[asNum] !== undefined) return asNum;
  const hit = Object.entries(labels).find(([, label]) => label.trim().toLowerCase() === raw.trim().toLowerCase());
  return hit ? Number(hit[0]) : undefined;
};

export interface ImportColumn {
  header: string;       // exact column header expected in the file
  required?: boolean;
  example?: string;     // sample value used in the downloadable template
}

export interface ImportModalProps<T> {
  open: boolean;
  onClose: () => void;
  title: string;
  templateName: string;                 // base file name for the downloaded template
  columns: ImportColumn[];
  /** Resolve + validate one parsed row (codes→ids via the caller's loaded lookups). */
  mapRow: (cells: Record<string, string>) => { item?: T; errors: string[] };
  commit: (items: T[]) => Promise<BulkImportResult>;
  onDone: () => void;                    // invalidate the list query after a successful import
}

interface PreviewRow<T> { rowNumber: number; cells: Record<string, string>; item?: T; errors: string[]; }

export default function ImportModal<T>(props: ImportModalProps<T>) {
  const { open, onClose, title, templateName, columns, mapRow, commit, onDone } = props;
  const { message } = AntApp.useApp();
  const [parsed, setParsed] = useState<ImportParseResult | null>(null);
  const [parsing, setParsing] = useState(false);
  const [committing, setCommitting] = useState(false);
  const [result, setResult] = useState<BulkImportResult | null>(null);

  const preview = useMemo<PreviewRow<T>[]>(() => {
    if (!parsed) return [];
    return parsed.rows.map((r) => ({ rowNumber: r.rowNumber, cells: r.cells, ...mapRow(r.cells) }));
  }, [parsed, mapRow]);

  const validRows = preview.filter((r) => r.errors.length === 0 && r.item !== undefined);
  const missingRequired = parsed
    ? columns.filter((c) => c.required && !parsed.columns.includes(c.header)).map((c) => c.header)
    : [];

  function reset() { setParsed(null); setResult(null); setParsing(false); setCommitting(false); }
  function close() { reset(); onClose(); }

  async function handleFile(file: File) {
    reset();
    setParsing(true);
    try {
      setParsed(await parseImportFile(file));
    } catch {
      message.error('Không đọc được tệp (định dạng .xlsx, .xml hoặc .csv)');
    } finally {
      setParsing(false);
    }
  }

  async function handleCommit() {
    setCommitting(true);
    try {
      const res = await commit(validRows.map((r) => r.item as T));
      setResult(res);
      if (res.created > 0) { message.success(`Đã nhập ${res.created} dòng`); onDone(); }
      if (res.failed > 0) message.warning(`${res.failed} dòng bị từ chối ở máy chủ`);
    } catch {
      message.error('Nhập thất bại (kiểm tra quyền)');
    } finally {
      setCommitting(false);
    }
  }

  function downloadTemplate() {
    const headers = columns.map((c) => c.header).join(',');
    const sample = columns.map((c) => c.example ?? '').join(',');
    const csv = `﻿${headers}\n${sample}\n`;          // BOM so Excel reads UTF-8
    const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' }));
    const a = document.createElement('a');
    a.href = url; a.download = `${templateName}.csv`; a.click();
    URL.revokeObjectURL(url);
  }

  return (
    <Modal title={title} open={open} onCancel={close} width={900} destroyOnClose
      footer={[
        <Button key="cancel" onClick={close}>Đóng</Button>,
        <Button key="ok" type="primary" disabled={validRows.length === 0} loading={committing}
          onClick={handleCommit}>
          Nhập {validRows.length} dòng hợp lệ
        </Button>,
      ]}>
      <Space direction="vertical" style={{ width: '100%' }} size="middle">
        <Space wrap>
          <Typography.Text type="secondary">
            Cột bắt buộc: {columns.filter((c) => c.required).map((c) => c.header).join(', ') || '—'}
          </Typography.Text>
          <Button size="small" onClick={downloadTemplate}>Tải mẫu CSV</Button>
        </Space>

        <Upload.Dragger accept=".xlsx,.xml,.csv" showUploadList={false} disabled={parsing}
          beforeUpload={(file) => { handleFile(file as unknown as File); return false; }}>
          <p className="ant-upload-drag-icon"><InboxOutlined /></p>
          <p className="ant-upload-text">Kéo thả hoặc bấm để chọn tệp (.xlsx, .xml, .csv)</p>
        </Upload.Dragger>

        {missingRequired.length > 0 && (
          <Alert type="error" showIcon message={`Thiếu cột bắt buộc: ${missingRequired.join(', ')}`} />
        )}

        {parsed && (
          <>
            <Alert type={validRows.length ? 'info' : 'warning'} showIcon
              message={`Tổng ${preview.length} dòng — ${validRows.length} hợp lệ, ${preview.length - validRows.length} lỗi`} />
            <Table<PreviewRow<T>>
              rowKey="rowNumber" size="small" dataSource={preview} scroll={{ x: true, y: 320 }}
              pagination={{ pageSize: 20, showSizeChanger: false }}
              columns={[
                { title: 'Dòng', dataIndex: 'rowNumber', width: 64, fixed: 'left' },
                {
                  title: 'Trạng thái', width: 150, fixed: 'left',
                  render: (_, r) => (r.errors.length === 0
                    ? <Tag color="green">Hợp lệ</Tag>
                    : <Tooltip title={r.errors.join('; ')}><Tag color="red">{r.errors.length} lỗi</Tag></Tooltip>),
                },
                ...(parsed.columns.map((col) => ({
                  title: col, dataIndex: ['cells', col],
                  render: (_: unknown, r: PreviewRow<T>) => r.cells[col] ?? '',
                }))),
              ]}
            />
          </>
        )}

        {result && (
          <Alert type={result.failed ? 'warning' : 'success'} showIcon
            message={`Kết quả: đã tạo ${result.created}, từ chối ${result.failed}`}
            description={result.errors.length > 0
              ? <ul style={{ margin: 0, paddingLeft: 18 }}>
                  {result.errors.slice(0, 20).map((e) => <li key={e.index}>Dòng {e.index + 1}: {e.message}</li>)}
                </ul>
              : undefined} />
        )}
      </Space>
    </Modal>
  );
}
