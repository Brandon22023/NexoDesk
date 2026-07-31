import type { CategoriaDto } from '../types/categorias'
import { httpRequest } from './http'

export function listarCategorias(
  signal?: AbortSignal,
): Promise<CategoriaDto[]> {
  // El formulario muestra solo las categorías disponibles para la organización de la sesión.
  return httpRequest<CategoriaDto[]>('/api/v1/categorias', { signal })
}
