import { useState } from 'react';
import { App as AntApp, Button, Input, Popconfirm, Space, Table, Upload } from 'antd';
import { UploadOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { deleteFile, downloadFile, FileResource, getFiles, uploadFile } from '../api/client';

const formatSize = (bytes: number) => {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
};

export default function FilesPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [category, setCategory] = useState('');

  const { data, isLoading } = useQuery({
    queryKey: ['files', page, pageSize, keyword],
    queryFn: () => getFiles({ page, pageSize, keyword }),
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ['files'] });
  const upload = useMutation({
    mutationFn: (v: { file: File; category?: string }) => uploadFile(v.file, v.category),
    onSuccess: () => { message.success('Đã tải lên'); invalidate(); },
    onError: () => message.error('Tải lên thất bại (kiểm tra quyền)'),
  });
  const remove = useMutation({
    mutationFn: deleteFile,
    onSuccess: () => { message.success('Đã xoá'); invalidate(); },
    onError: () => message.error('Không xoá được'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space wrap>
        <Input.Search placeholder="Tìm theo tên tệp" allowClear style={{ width: 280 }}
          onSearch={(v) => { setKeyword(v); setPage(1); }} />
        <Input placeholder="Phân loại (tuỳ chọn)" allowClear style={{ width: 200 }}
          value={category} onChange={(e) => setCategory(e.target.value)} />
        <Upload showUploadList={false} disabled={upload.isPending}
          beforeUpload={(file) => { upload.mutate({ file: file as unknown as File, category: category || undefined }); return false; }}>
          <Button type="primary" icon={<UploadOutlined />} loading={upload.isPending}>Tải tệp lên</Button>
        </Upload>
      </Space>

      <Table<FileResource>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Tên tệp', dataIndex: 'fileName' },
          { title: 'Phân loại', dataIndex: 'category', width: 160, render: (c: string | null) => c ?? '—' },
          { title: 'Kích thước', dataIndex: 'sizeBytes', width: 120, render: (s: number) => formatSize(s) },
          { title: 'Người tải', dataIndex: 'uploadedBy', width: 160, render: (u: string | null) => u ?? '—' },
          { title: 'Thời điểm', dataIndex: 'uploadedAtUtc', width: 180,
            render: (t: string) => new Date(t).toLocaleString('vi-VN') },
          { title: 'Thao tác', width: 160, render: (_, r) => (
            <Space>
              <a onClick={() => downloadFile(r.id, r.fileName)}>Tải xuống</a>
              <Popconfirm title="Xoá tệp này?" okText="Xoá" cancelText="Huỷ" onConfirm={() => remove.mutate(r.id)}>
                <a style={{ color: '#cf1322' }}>Xoá</a>
              </Popconfirm>
            </Space>) },
        ]}
      />
    </Space>
  );
}
