import { useState } from 'react';

export function useSubmit(action) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState('');

  const submit = async (...args) => {
    setSubmitError('');
    setIsSubmitting(true);
    try {
      await action(...args);
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : 'No fue posible completar la operación.');
      setIsSubmitting(false);
    }
  };

  return { submit, isSubmitting, submitError };
}
