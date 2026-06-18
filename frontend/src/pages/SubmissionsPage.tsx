import { useState } from 'react';
import { App as AntApp, Button, Form, Input, Modal, Select, Space, Table, Tag, Timeline } from 'antd';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  createSubmission, getCampaigns, getOrgUnits, getSubmissionDetail, getSubmissions,
  ReportAction, ReportActionValue, ReportState, Submission, submissionAction,
} from '../api/client';

const stateLabels: Record<ReportState, string> = {
  1: 'Nháp', 2: 'Đã gửi', 3: 'Đang thẩm định', 4: 'Chờ phê duyệt', 5: 'Đã duyệt', 6: 'Bị từ chối',
};
const stateColors: Record<ReportState, string> = {
  1: 'default', 2: 'blue', 3: 'cyan', 4: 'gold', 5: 'green', 6: 'red',
};

// Which workflow actions are offered for each state.
const actionsByState: Record<ReportState, { action: ReportActionValue; label: string; danger?: boolean }[]> = {
  1: [{ action: ReportAction.Submit, label: 'Gửi' }],
  2: [{ action: ReportAction.AcceptForReview, label: 'Tiếp nhận' }, { action: ReportAction.Return, label: 'Trả lại', danger: true }],
  3: [{ action: ReportAction.ForwardForApproval, label: 'Trình duyệt' }, { action: ReportAction.Return, label: 'Trả lại', danger: true }],
  4: [{ action: ReportAction.Approve, label: 'Phê duyệt' }, { action: ReportAction.Reject, label: 'Từ chối', danger: true }],
  5: [],
  6: [{ action: ReportAction.Reopen, label: 'Mở lại' }],
};

export default function SubmissionsPage() {
  const { message } = AntApp.useApp();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [open, setOpen] = useState(false);
  const [historyId, setHistoryId] = useState<string | null>(null);
  const [form] = Form.useForm();

  const { data, isLoading } = useQuery({
    queryKey: ['submissions', page, pageSize],
    queryFn: () => getSubmissions({ page, pageSize }),
  });
  const { data: campaigns } = useQuery({ queryKey: ['campaigns', 'all'], queryFn: () => getCampaigns({ page: 1, pageSize: 100 }) });
  const { data: units } = useQuery({ queryKey: ['org-units', 'all'], queryFn: () => getOrgUnits({ page: 1, pageSize: 100 }) });
  const { data: detail } = useQuery({
    queryKey: ['submission', historyId],
    queryFn: () => getSubmissionDetail(historyId!),
    enabled: !!historyId,
  });

  const campaignName = (id: string) => campaigns?.items.find((c) => c.id === id)?.name ?? id;
  const unitName = (id: string) => units?.items.find((u) => u.id === id)?.name ?? id;

  const create = useMutation({
    mutationFn: createSubmission,
    onSuccess: () => {
      message.success('Đã tạo báo cáo');
      setOpen(false); form.resetFields();
      qc.invalidateQueries({ queryKey: ['submissions'] });
    },
    onError: () => message.error('Tạo thất bại (kiểm tra quyền/phạm vi dữ liệu)'),
  });

  const act = useMutation({
    mutationFn: ({ id, action }: { id: string; action: ReportActionValue }) => submissionAction(id, action),
    onSuccess: () => {
      message.success('Đã cập nhật trạng thái');
      qc.invalidateQueries({ queryKey: ['submissions'] });
    },
    onError: () => message.error('Thao tác không hợp lệ ở trạng thái hiện tại hoặc thiếu quyền'),
  });

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="middle">
      <Space>
        <Button type="primary" onClick={() => setOpen(true)}>Tạo báo cáo</Button>
      </Space>

      <Table<Submission>
        rowKey="id" loading={isLoading} dataSource={data?.items}
        pagination={{ current: page, pageSize, total: data?.totalCount, showSizeChanger: true,
          pageSizeOptions: [10, 20, 50], onChange: (p, ps) => { setPage(p); setPageSize(ps); } }}
        columns={[
          { title: 'Tiêu đề', dataIndex: 'title' },
          { title: 'Kỳ báo cáo', dataIndex: 'campaignId', width: 200, render: (id: string) => campaignName(id) },
          { title: 'Đơn vị', dataIndex: 'orgUnitId', width: 180, render: (id: string) => unitName(id) },
          { title: 'Trạng thái', dataIndex: 'state', width: 150,
            render: (s: ReportState) => <Tag color={stateColors[s]}>{stateLabels[s]}</Tag> },
          {
            title: 'Thao tác', width: 320,
            render: (_, r) => (
              <Space wrap>
                {actionsByState[r.state].map((a) => (
                  <Button key={a.action} size="small" danger={a.danger} loading={act.isPending}
                    onClick={() => act.mutate({ id: r.id, action: a.action })}>
                    {a.label}
                  </Button>
                ))}
                <Button size="small" type="link" onClick={() => setHistoryId(r.id)}>Lịch sử</Button>
              </Space>
            ),
          },
        ]}
      />

      <Modal title="Tạo báo cáo" open={open} onCancel={() => setOpen(false)}
        onOk={() => form.submit()} confirmLoading={create.isPending}>
        <Form form={form} layout="vertical" onFinish={(v) => create.mutate(v)}>
          <Form.Item name="campaignId" label="Kỳ báo cáo" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label"
              options={campaigns?.items.map((c) => ({ value: c.id, label: c.name }))} />
          </Form.Item>
          <Form.Item name="orgUnitId" label="Đơn vị (trong phạm vi của bạn)" rules={[{ required: true }]}>
            <Select showSearch optionFilterProp="label"
              options={units?.items.map((u) => ({ value: u.id, label: `${u.name} (${u.code})` }))} />
          </Form.Item>
          <Form.Item name="title" label="Tiêu đề" rules={[{ required: true }]}><Input maxLength={300} /></Form.Item>
        </Form>
      </Modal>

      <Modal title="Lịch sử xử lý" open={!!historyId} footer={null} onCancel={() => setHistoryId(null)}>
        <Timeline
          items={detail?.history.map((h) => ({
            children: (
              <div>
                <b>{h.action}</b>{h.actorName ? ` — ${h.actorName}` : ''}<br />
                <span style={{ color: '#888' }}>{new Date(h.atUtc).toLocaleString('vi-VN')}</span>
                {h.note ? <div>Ghi chú: {h.note}</div> : null}
              </div>
            ),
          }))}
        />
      </Modal>
    </Space>
  );
}
