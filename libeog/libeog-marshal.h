
#ifndef __libeog_marshal_MARSHAL_H__
#define __libeog_marshal_MARSHAL_H__

#include	<glib-object.h>

G_BEGIN_DECLS

/* VOID:INT,INT,INT,INT (./libeog-marshal.list:1) */
extern void libeog_marshal_VOID__INT_INT_INT_INT (GClosure     *closure,
                                                  GValue       *return_value,
                                                  guint         n_param_values,
                                                  const GValue *param_values,
                                                  gpointer      invocation_hint,
                                                  gpointer      marshal_data);

/* VOID:INT,INT (./libeog-marshal.list:2) */
extern void libeog_marshal_VOID__INT_INT (GClosure     *closure,
                                          GValue       *return_value,
                                          guint         n_param_values,
                                          const GValue *param_values,
                                          gpointer      invocation_hint,
                                          gpointer      marshal_data);

/* VOID:OBJECT,OBJECT (./libeog-marshal.list:3) */
extern void libeog_marshal_VOID__OBJECT_OBJECT (GClosure     *closure,
                                                GValue       *return_value,
                                                guint         n_param_values,
                                                const GValue *param_values,
                                                gpointer      invocation_hint,
                                                gpointer      marshal_data);

/* VOID:POINTER (./libeog-marshal.list:4) */
#define libeog_marshal_VOID__POINTER	g_cclosure_marshal_VOID__POINTER

/* VOID:DOUBLE (./libeog-marshal.list:5) */
#define libeog_marshal_VOID__DOUBLE	g_cclosure_marshal_VOID__DOUBLE

G_END_DECLS

#endif /* __libeog_marshal_MARSHAL_H__ */

