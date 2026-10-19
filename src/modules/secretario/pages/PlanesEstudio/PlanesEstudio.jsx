import { useMemo, useState } from 'react';
import { Plus, X } from 'lucide-react';
import PlanCard from './PlanCard';
import CrearPlanModal from './CreatePlanModal';
import PlanDetalle from './PlanDetail';
import { plansApi } from '../../components/plans/plansApi';
import { useAsyncResource } from '../../../../hooks/useAsyncResource';

const filters = [
  { id: 'todos', label: 'Todos' },
  { id: 'activos', label: 'Activos' },
  { id: 'archivados', label: 'Archivados' },
];

function PlanesEstudio() {
  const { data: planes, setData: setPlanes, isLoading, error } = useAsyncResource(plansApi.obtenerPlanes, []);
  const [notice, setNotice] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedPlanId, setSelectedPlanId] = useState(null);
  const [filter, setFilter] = useState('todos');

  const activePlans = useMemo(() => planes.filter((plan) => plan.activo), [planes]);
  const archivedPlans = useMemo(() => planes.filter((plan) => !plan.activo), [planes]);

  const selectedPlan = useMemo(
    () => planes.find((plan) => plan.id === selectedPlanId) ?? null,
    [planes, selectedPlanId]
  );

  const handleCreatePlan = async (formData) => {
    const newPlan = await plansApi.crearPlan(formData);
    setPlanes((currentPlans) => [newPlan, ...currentPlans]);
    setNotice(`Se creó el plan "${newPlan.nombre}".`);
    setIsModalOpen(false);
  };

  const togglePlanStatus = async (planId) => {
    const plan = planes.find((currentPlan) => currentPlan.id === planId);
    if (!plan) return;
    await plansApi.actualizarPlan(planId, { activo: !plan.activo });
    setPlanes((currentPlans) => currentPlans.map((currentPlan) => (currentPlan.id === planId ? { ...currentPlan, activo: !currentPlan.activo } : currentPlan)));
  };

  if (selectedPlan) {
    return <PlanDetalle plan={selectedPlan} onBack={() => setSelectedPlanId(null)} />;
  }

  const renderSection = (title, plans) => (
    <div className="plans-section">
      <h2>{title}</h2>
      <div className="plans-grid">
        {plans.map((plan) => (
          <PlanCard key={plan.id} plan={plan} onToggleStatus={togglePlanStatus} onViewSubjects={setSelectedPlanId} />
        ))}
      </div>
      {plans.length === 0 && <p className="plans-empty">No hay planes en esta sección.</p>}
    </div>
  );

  return (
    <section className="planes-page">
      <div className="planes-heading">
        <div>
          <h1>Planes de Estudio</h1>
          <p>Gestiona los planes de estudio de la carrera.</p>
        </div>
        <button className="create-plan-button" type="button" onClick={() => setIsModalOpen(true)}>
          <Plus size={16} /> Crear nuevo plan
        </button>
      </div>

      <div className="plans-tabs" role="tablist">
        {filters.map((item) => (
          <button
            key={item.id}
            type="button"
            role="tab"
            aria-selected={filter === item.id}
            className={filter === item.id ? 'active' : ''}
            onClick={() => setFilter(item.id)}
          >
            {item.label}
          </button>
        ))}
      </div>

      {notice && (
        <div className="planes-notice" role="status">
          {notice}
          <button type="button" aria-label="Cerrar aviso" onClick={() => setNotice('')}><X size={16} /></button>
        </div>
      )}

      {error && <p role="alert">{error}</p>}
      {isLoading && <p>Cargando planes...</p>}

      {!isLoading && filter !== 'archivados' && renderSection('Planes Activos', activePlans)}
      {!isLoading && filter !== 'activos' && renderSection('Planes Archivados', archivedPlans)}

      {isModalOpen && <CrearPlanModal onClose={() => setIsModalOpen(false)} onCreate={handleCreatePlan} />}
    </section>
  );
}

export default PlanesEstudio;