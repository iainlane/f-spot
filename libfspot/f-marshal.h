
#ifndef __f_marshal_MARSHAL_H__
#define __f_marshal_MARSHAL_H__

#include	<glib-object.h>

G_BEGIN_DECLS

/* NONE:INT (./f-marshal.list:1) */
#define f_marshal_VOID__INT	g_cclosure_marshal_VOID__INT
#define f_marshal_NONE__INT	f_marshal_VOID__INT

/* NONE:NONE (./f-marshal.list:2) */
#define f_marshal_VOID__VOID	g_cclosure_marshal_VOID__VOID
#define f_marshal_NONE__NONE	f_marshal_VOID__VOID

/* NONE:STRING,INT (./f-marshal.list:3) */
extern void f_marshal_VOID__STRING_INT (GClosure     *closure,
                                        GValue       *return_value,
                                        guint         n_param_values,
                                        const GValue *param_values,
                                        gpointer      invocation_hint,
                                        gpointer      marshal_data);
#define f_marshal_NONE__STRING_INT	f_marshal_VOID__STRING_INT

/* NONE:STRING,INT,POINTER (./f-marshal.list:4) */
extern void f_marshal_VOID__STRING_INT_POINTER (GClosure     *closure,
                                                GValue       *return_value,
                                                guint         n_param_values,
                                                const GValue *param_values,
                                                gpointer      invocation_hint,
                                                gpointer      marshal_data);
#define f_marshal_NONE__STRING_INT_POINTER	f_marshal_VOID__STRING_INT_POINTER

/* NONE:STRING,POINTER (./f-marshal.list:5) */
extern void f_marshal_VOID__STRING_POINTER (GClosure     *closure,
                                            GValue       *return_value,
                                            guint         n_param_values,
                                            const GValue *param_values,
                                            gpointer      invocation_hint,
                                            gpointer      marshal_data);
#define f_marshal_NONE__STRING_POINTER	f_marshal_VOID__STRING_POINTER

/* NONE:STRING,STRING (./f-marshal.list:6) */
extern void f_marshal_VOID__STRING_STRING (GClosure     *closure,
                                           GValue       *return_value,
                                           guint         n_param_values,
                                           const GValue *param_values,
                                           gpointer      invocation_hint,
                                           gpointer      marshal_data);
#define f_marshal_NONE__STRING_STRING	f_marshal_VOID__STRING_STRING

/* NONE:STRING,STRING,POINTER,BOOL (./f-marshal.list:7) */
extern void f_marshal_VOID__STRING_STRING_POINTER_BOOLEAN (GClosure     *closure,
                                                           GValue       *return_value,
                                                           guint         n_param_values,
                                                           const GValue *param_values,
                                                           gpointer      invocation_hint,
                                                           gpointer      marshal_data);
#define f_marshal_NONE__STRING_STRING_POINTER_BOOL	f_marshal_VOID__STRING_STRING_POINTER_BOOLEAN

/* NONE:UINT (./f-marshal.list:8) */
#define f_marshal_VOID__UINT	g_cclosure_marshal_VOID__UINT
#define f_marshal_NONE__UINT	f_marshal_VOID__UINT

G_END_DECLS

#endif /* __f_marshal_MARSHAL_H__ */

