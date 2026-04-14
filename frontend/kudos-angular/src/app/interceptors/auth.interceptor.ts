import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const token = typeof window !== 'undefined'
    ? window.localStorage.getItem('kudos_token')
    : null;

  if (!token) {
    return next(request);
  }

  const clonedRequest = request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(clonedRequest);
};